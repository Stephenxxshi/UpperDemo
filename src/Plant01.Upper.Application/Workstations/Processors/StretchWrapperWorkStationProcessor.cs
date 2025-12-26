using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Application.Workstations.Processors;

/// <summary>
/// 出垛工位流程处理器示例
/// </summary>
public class StretchWrapperWorkStationProcessor : WorkstationProcessorBase
{
    public StretchWrapperWorkStationProcessor(IDeviceCommunicationService deviceComm, IMesService mesService, IEquipmentConfigService equipmentConfigService, IServiceScopeFactory serviceScopeFactory, IServiceProvider serviceProvider, IWorkOrderRepository workOrderRepository, ILogger<WorkstationProcessorBase> logger) : base(deviceComm, mesService, equipmentConfigService, serviceScopeFactory, serviceProvider, workOrderRepository, logger)
    {
        WorkstationType = "StretchWrapper";
    }

    protected override async Task InternalExecuteAsync(WorkstationProcessContext context, string bagCode)
    {

        // 发送mesService

        // 判断重量是否合格

        // 回复PLC
        await WriteProcessResult(context, ProcessResult.Success, "出垛成功");
        _logger.LogInformation("出垛工位流程执行完成");

    }


}
