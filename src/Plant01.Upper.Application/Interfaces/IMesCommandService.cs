namespace Plant01.Upper.Application.Interfaces;

public interface IMesCommandService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
