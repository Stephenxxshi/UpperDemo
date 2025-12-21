using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Infrastructure.Workstations.Processors;

/// <summary>
/// 出垛工位流程处理器示例
/// </summary>
public class PalletOutWorkstationProcessor : IWorkstationProcessor
{
    public string WorkstationType => "PalletOut";

    private readonly IDeviceCommunicationService _deviceComm;
    private readonly IMesService _mesService;
    private readonly ILogger<PackagingWorkstationProcessor> _logger;
    private readonly IEquipmentConfigService _equipmentConfigService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PalletOutWorkstationProcessor(
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
        _logger.LogInformation("[ {Workstation} ] 出垛工位触发流程: , 触发标签: [ {Tag} ]",
            context.WorkstationCode, context.TriggerTagName);

        try
        {
            // 获取工单
            var workOrders = await _workOrderRepository.GetAllAsync(workOrder => workOrder.Status == Domain.ValueObjects.WorkOrderStatus.开工);

            var currentWorkOrder = workOrders.First();

            // 获取设备配置以查找标签
            var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
            if (equipment == null)
            {
                _logger.LogError("[ {Code} ] : 未找到设备配置 ", context.EquipmentCode);
                return;
            }

            // 获取袋码和托盘号标签
            var qrCodeTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "QrCode" || m.TagName.EndsWith(".Code"));
            var palletTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "PalletCode");
            
            // 获取袋码和托号号值
            string bagCode = string.Empty;
            string pallet = string.Empty;
            if (qrCodeTag is not null && palletTag is not null)
            {
                bagCode = _deviceComm.GetTagValue<string>(qrCodeTag.TagName);
                pallet = _deviceComm.GetTagValue<string>(palletTag.TagName);
            }

            if (string.IsNullOrEmpty(bagCode) && string.IsNullOrEmpty(pallet))
            {
                _logger.LogWarning("包装机触发但未读取到袋码: {Code}", context.EquipmentCode);
                return;
            }

            // 查询所有工单的代码
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var bagRepo = unitOfWork.BagRepository;
            var bags = await bagRepo.GetAllAsync(o => o.OrderCode == currentWorkOrder.Code);
            string bagsStr = string.Join(",", bags.Select(o=>o.BagCode));

            // 发送给MES出垛信息

            FinishPalletizingRequest request = new FinishPalletizingRequest()
            {
                AgvDeviceCode = "AGV1",
                DeviceCode = "MDJ1",
                JobNo = currentWorkOrder.Code,
                List = new List<PackageDetail>()
                  {
                      new PackageDetail(){ BagNums = bagsStr, Quan = bags.Count}
                  },
                PalletId = pallet
            };
            var response = await _mesService.FinishPalletizingAsync(request);
            if (response != null)
            {
            }



            // 保存袋码

            _logger.LogInformation("出垛工位流程执行完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "出垛工位流程执行失败");
            throw;
        }
    }
}
