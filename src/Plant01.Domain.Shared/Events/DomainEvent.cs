namespace Plant01.Domain.Shared.Events
{
    public class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// 领域事件唯一标识
        /// </summary>
        public Guid Id { get; protected set; }
        
        /// <summary>
        /// 事件发生时间
        /// </summary>
        public DateTime OccurredOn { get; protected set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.Now;
        }

    }
}
