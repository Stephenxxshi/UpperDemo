using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 贴标工位流程处理器示例
/// </summary>
public class LabelingWorkStationProcessor : WorkstationProcessorBase
{
    public LabelingWorkStationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger)
    {
        WorkstationType = "WS_Inkjet";
        WorkStationProcess = "贴标工位流程";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {

        // 获取

        // 发送喷码内容

        // 保存袋码

        // 发送PLC
        await WriteProcessResult(context, ProcessResult.Success, "出垛成功");
        _logger.LogInformation($"[ {WorkStationProcess} ] 袋码 [ {bagCode} ] -> 完成喷码");
    }


}
