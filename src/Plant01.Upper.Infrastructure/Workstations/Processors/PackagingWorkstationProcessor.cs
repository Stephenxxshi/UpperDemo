using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Infrastructure.Workstations.Processors;

/// <summary>
/// 包装工位流程处理器示例
/// </summary>
public class PackagingWorkstationProcessor : IWorkstationProcessor
{
    public string WorkstationType => "Packaging";

    private readonly IDeviceCommunicationService _deviceComm;
    private readonly IMesService _mesService;
    private readonly ILogger<PackagingWorkstationProcessor> _logger;
    private readonly IEquipmentConfigService _equipmentConfigService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PackagingWorkstationProcessor(
        IDeviceCommunicationService deviceComm,
        IMesService mesService,
        IEquipmentConfigService equipmentConfigService,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        IWorkOrderRepository workOrderRepository,
        ILogger<PackagingWorkstationProcessor> logger)
    {
        _deviceComm = deviceComm;
        _mesService = mesService;
        _logger = logger;
        _workOrderRepository = workOrderRepository;
        _equipmentConfigService = equipmentConfigService;
        _serviceProvider = serviceProvider;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ExecuteAsync(WorkstationProcessContext context)
    {
        _logger.LogInformation("开始执行包装工位流程: {Workstation}, 触发标签: {Tag}",
            context.WorkstationCode, context.TriggerTagName);

        try
        {
            // 读取工单号
            var workOrders = await _workOrderRepository.GetAllAsync(workOrder => workOrder.Status == Domain.ValueObjects.WorkOrderStatus.开工);
            if (workOrders.Count == 0)
            {
                _logger.LogWarning($"{context.EquipmentCode} : 没有找到开工中的工单");
                return;
            }

            if (workOrders.Count > 1)
            {
                _logger.LogError($"{context.EquipmentCode} : 开工的工单数量为 [ {workOrders.Count} ] > 1");
                return;
            }

            var currentWorkOrder = workOrders.First();

            // 获取设备配置以查找标签
            var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
            if (equipment == null)
            {
                _logger.LogError("未找到设备配置: {Code}", context.EquipmentCode);
                return;
            }

            // 获取袋码标签
            var qrCodeTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "QrCode" || m.TagName.EndsWith(".Code"));
            string bagCode = string.Empty;

            if (qrCodeTag != null)
            {
                bagCode = _deviceComm.GetTagValue<string>(qrCodeTag.TagName);
            }

            if (string.IsNullOrEmpty(bagCode))
            {
                _logger.LogWarning("包装机触发但未读取到袋码: {Code}", context.EquipmentCode);
                return;
            }

            // 保存袋码
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var bagRepo = unitOfWork.BagRepository;

            var bags = await bagRepo.GetAllAsync(o => o.BagCode == bagCode);
            var bag = bags.FirstOrDefault();
            bool isNew = false;

            if (bag == null)
            {
                // 上袋是第一步，如果不存在则创建
                bag = new Bag
                {
                    BagCode = bagCode,
                    CreatedAt = DateTime.Now,
                    OrderCode = currentWorkOrder.Code,
                    ProductCode = currentWorkOrder.ProductCode,
                    ProductAlias = currentWorkOrder.ProductName,
                    LineNo = currentWorkOrder.LineNo,
                    StationNo = context.WorkstationCode,
                    ProductWeightUnit = "kg", // 默认单位
                    ProductHeightUnit = "mm",  // 默认单位
                    BatchCode = currentWorkOrder.BatchNumber
                };
                await bagRepo.AddAsync(bag);
                isNew = true;
            }

            if (bag.CanLoad())
            {
                bag.AddRecord(ProcessStep.Loading, context.EquipmentCode, true);

                if (!isNew)
                {
                    await bagRepo.UpdateAsync(bag);
                }

                await unitOfWork.SaveChangesAsync();
                _logger.LogInformation("袋 {BagCode} 在 {MachineId} 加载", bagCode, context.EquipmentCode);
            }

            _logger.LogInformation("包装工位流程执行完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "包装工位流程执行失败");
            throw;
        }
    }
}
