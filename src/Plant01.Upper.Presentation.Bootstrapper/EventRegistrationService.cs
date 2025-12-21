using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plant01.Domain.Shared.Events;
using Plant01.Upper.Application.EventHandlers;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Domain.Events;

namespace Plant01.Upper.Presentation.Bootstrapper;

public class EventRegistrationService : IHostedService
{
    private readonly IDomainEventBus _eventBus;
    private readonly IServiceScopeFactory _scopeFactory;

    public EventRegistrationService(
        IDomainEventBus eventBus, 
        IServiceScopeFactory scopeFactory)
    {
        _eventBus = eventBus;
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 注册 PalletDischargedEvent -> MesEventHandler
        _eventBus.Register<PalletDischargedEvent>(async e => 
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<MesEventHandler>();
            await handler.HandlePalletDischargedAsync(e);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
