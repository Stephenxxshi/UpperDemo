namespace Plant01.Upper.Application.Interfaces;

/// <summary>
/// MES Web API 服务接口 - 接收MES生产工单推送
/// </summary>
public interface IMesWebApi
{
    /// <summary>
    /// 启动 Web API 服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止 Web API 服务
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 收到工单推送事件
    /// </summary>
    event Func<WorkOrderRequestDto, Task<WorkOrderResponse>>? OnWorkOrderReceived;

    /// <summary>
    /// 服务是否正在运行
    /// </summary>
    bool IsRunning { get; }
}



