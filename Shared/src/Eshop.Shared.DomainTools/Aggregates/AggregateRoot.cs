using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Shared.DomainTools.Aggregates;

public abstract class AggregateRoot<TKey> : IEntityBase<TKey>, IAggregateRoot<TKey>
{
    public abstract TKey Id { get; set; }

    private readonly List<IDomainEvent> _uncommittedDomainEvents = new();

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _uncommittedDomainEvents.ToList();

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _uncommittedDomainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _uncommittedDomainEvents.Clear();
    }
}