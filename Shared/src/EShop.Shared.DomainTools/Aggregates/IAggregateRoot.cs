using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Shared.DomainTools.Aggregates
{
    public interface IAggregateRoot
    {
        IReadOnlyCollection<IDomainEvent> GetDomainEvents();
        void ClearDomainEvents();
        void RaiseDomainEvent(IDomainEvent domainEvent);
    }

    public interface IAggregateRoot<TKey> : IAggregateRoot, IEntityBase<TKey>
    {
    }

    public abstract class AggregateRoot<TKey> : EntityBase<TKey>, IAggregateRoot<TKey>
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.ToList();
        }

        public void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
            {
                ArgumentNullException.ThrowIfNull(domainEvent);
            }

            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
