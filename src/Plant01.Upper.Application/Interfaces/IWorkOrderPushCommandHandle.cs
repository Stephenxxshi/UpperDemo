namespace Plant01.Upper.Application.Interfaces;

public interface IWorkOrderPushCommandHandle
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
