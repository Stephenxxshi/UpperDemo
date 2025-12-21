using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Infrastructure.Workstations.Processors;

/// <summary>
/// 工位流程处理器基类
/// </summary>
public abstract class WorkstationProcessorBase : IWorkstationProcessor
{
    public string WorkstationType { get; protected set; } = string.Empty;

    protected readonly IDeviceCommunicationService _deviceComm;
    protected readonly IMesService _mesService;
    protected readonly ILogger<WorkstationProcessorBase> _logger;
    protected readonly IEquipmentConfigService _equipmentConfigService;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IWorkOrderRepository _workOrderRepository;
    protected readonly IServiceScopeFactory _serviceScopeFactory;

    public WorkstationProcessorBase(
        IDeviceCommunicationService deviceComm,
        IMesService mesService,
        IEquipmentConfigService equipmentConfigService,
        IServiceScopeFactory serviceScopeFactory,
        IServiceProvider serviceProvider,
        IWorkOrderRepository workOrderRepository,
        ILogger<WorkstationProcessorBase> logger)
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
        _logger.LogInformation("[ {Tag} ] : 工位 [ {Workstation} ] -> 触发流程", context.TriggerTagName, context.WorkstationCode);

        await InternalExecuteAsync();

        try
        {
            // 读取工单号
            var workOrders = await _workOrderRepository.GetAllAsync(workOrder => workOrder.Status == Domain.ValueObjects.WorkOrderStatus.开工);
            if (workOrders.Count == 0)
            {
                _logger.LogWarning($"[ {context.TriggerTagName} ] ->  未找到开工中的工单");
                return;
            }

            if (workOrders.Count > 1)
            {
                _logger.LogError($"[ {context.TriggerTagName} ] -> 开工的工单数量为 [ {workOrders.Count} ] > 1");
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

            await WriteProcessResult(context.EquipmentCode, ProcessResult.Success);

            _logger.LogInformation("包装工位流程执行完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "包装工位流程执行失败");
            throw;
        }
    }

    /// <summary>
    /// 写回流程结果到PLC
    /// </summary>
    protected async Task WriteProcessResult(string equipmentCode, ProcessResult result, string? message = null)
    {
        try
        {
            var equipment = _equipmentConfigService.GetEquipment(equipmentCode);
            if (equipment == null)
                return;

            // 查找 ProcessResult 用途的标签
            var resultMapping = equipment.TagMappings
                .FirstOrDefault(m => m.Purpose == TagPurpose.ProcessResult);

            if (resultMapping != null)
            {
                await _deviceComm.WriteTagAsync(resultMapping.TagName, (int)result);
                _logger.LogInformation($"[ {resultMapping.TagName} ] ： 写入 -> {result}");
            }

            // 如果有消息标签，也写回消息
            if (!string.IsNullOrEmpty(message))
            {
                var messageMapping = equipment.TagMappings
                    .FirstOrDefault(m => m.TagName.Contains("Message") || m.TagName.Contains("Msg"));

                if (messageMapping != null)
                {
                    await _deviceComm.WriteTagAsync(messageMapping.TagName, message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写回流程结果失败: {Equipment}", equipmentCode);
        }
    }

    protected virtual async Task InternalExecuteAsync()
    {
        return;
    }
}
