using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Shared.DomainTools.Aggregates;

public interface IAggregateRoot<TKey> : IEntityBase<TKey>
{
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents();

    public void ClearDomainEvents();

    public void Raise(IDomainEvent domainEvent);
}