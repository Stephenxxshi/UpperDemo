using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 出垛工位流程处理器示例
/// </summary>
public class CheckweigherWorkStationProcessor : WorkstationProcessorBase
{
    public CheckweigherWorkStationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger)
    {
        WorkstationType = "Checkweigher";
        WorkStationProcess = "复检称重流程";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {

        // 获取重量

        // 判断重量是否合格

        // 保存袋码
        await WriteProcessResult(context, ProcessResult.Success, "出垛成功");
        _logger.LogInformation($"[ WorkStationProcess ] 袋码[ {bagCode} ] -> 出垛工位流程执行完成");

        // 发送PLC
    }


}
