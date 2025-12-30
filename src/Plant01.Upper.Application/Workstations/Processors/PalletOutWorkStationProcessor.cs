using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 出垛工位流程处理器示例
/// </summary>
public class PalletOutWorkStationProcessor : WorkstationProcessorBase
{
    public PalletOutWorkStationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger)
    {
        WorkstationType = "WS_PalletOut";
        WorkStationProcess = "出垛工位流程";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {
        // 通过袋码获取所在托盘

        // 判断托盘是否满垛

        // 获取工单
        var workOrders = await _workOrderRepository.GetAllAsync(workOrder => workOrder.Status == Domain.ValueObjects.WorkOrderStatus.开工);

        var currentWorkOrder = workOrders.First();

        // 获取设备配置以查找标签
        var equipment = _equipmentConfigService.GetEquipment(context.EquipmentCode);
        if (equipment == null)
        {
            _logger.LogError($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] : 未找到设备配置 ", context.EquipmentCode);
            await WriteProcessResult(context, ProcessResult.Error, "没有找到设备配置");
            return;
        }

        // 获取托盘号标签
        var palletTag = equipment.TagMappings.FirstOrDefault(m => m.Purpose == "PalletCode");
        if (palletTag is null)
        {
            _logger.LogError($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 在 {context.EquipmentCode}  未找到 PalletCode 功能标签");
            await WriteProcessResult(context, ProcessResult.Error, "未找到 PalletCode 功能标签");
            return;
        }

        // 获取托盘号
        string pallet = _deviceComm.GetTagValue<string>(palletTag.TagCode);
        if (string.IsNullOrEmpty(pallet))
        {
            _logger.LogWarning($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] -> 未读取到PLC的托盘号");
            await WriteProcessResult(context, ProcessResult.Error, "未读取到PLC的托盘号");
            return;
        }
        _logger.LogInformation($"[ {WorkStationProcess} ] 袋码[ {bagCode} ]  在 {context.EquipmentCode}  读取到托盘号: {pallet}");



        // 查询所有工单的代码
        using var scope = _serviceScopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var bagRepo = unitOfWork.BagRepository;
        var bags = await bagRepo.GetAllAsync(o => o.OrderCode == currentWorkOrder.Code);
        string bagsStr = string.Join(",", bags.Select(o => o.BagCode));

        // 

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
            _logger.LogError($"[ {WorkStationProcess} ] 袋码[ {bagCode} ] : {response.ErrorMsg}");
            await WriteProcessResult(context, ProcessResult.Error, response.ErrorMsg);
            return;
        }



        // 保存袋码
        await WriteProcessResult(context, ProcessResult.Success, "出垛成功");
        _logger.LogInformation($"[ {WorkStationProcess} ] 出垛工位流程执行完成");
    }


}
