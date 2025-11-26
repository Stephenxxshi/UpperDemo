namespace Plant01.Domain.Shared.Events
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }

    // 基础实现（可选）
    public abstract class DomainEventBase : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }

}
