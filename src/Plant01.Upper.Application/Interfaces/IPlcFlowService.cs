namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// PLC 流程控制服务接口
/// </summary>
public interface IPlcFlowService
{
    // 现在的架构改为基于消息驱动 (StationTriggerMessage)
    // 此接口保留作为服务标记，或用于将来添加非消息驱动的管理方法
}
