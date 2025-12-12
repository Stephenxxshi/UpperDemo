using System.Collections.Concurrent;
using Plant01.Domain.Shared.Events;

namespace Plant01.Upper.Infrastructure.Services;

/// <summary>
/// 简单的内存领域事件总线实现
/// </summary>
public class DomainEventBus : IDomainEventBus
{
    // 存储事件处理器: EventType -> List<HandlerDelegate>
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                if (handler is Func<TEvent, Task> asyncAction)
                {
                    await asyncAction(domainEvent);
                }
                else if (handler is Action<TEvent> action)
                {
                    action(domainEvent);
                }
            }
        }
    }

    public void Register<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(eventType, 
            _ => new List<Delegate> { handler }, 
            (_, list) => { list.Add(handler); return list; });
    }

    public void Register<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(eventType, 
            _ => new List<Delegate> { handler }, 
            (_, list) => { list.Add(handler); return list; });
    }

    public void Unregister<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
}
