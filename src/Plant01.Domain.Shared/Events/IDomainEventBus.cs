namespace Plant01.Domain.Shared.Events
{
    public interface IDomainEventBus
    {
        Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
            where TEvent : IDomainEvent;
        void Register<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent;

        void Register<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
        void Unregister<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
    }
}
