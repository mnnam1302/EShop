using Eshop.Shared.DomainTools.Entities;
using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace Eshop.Shared.DomainTools.Aggregates;

public interface IAggregateRoot<TKey> : IEntityBase<TKey>
{
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents();

    public void ClearDomainEvents();

    public void RaiseDomainEvent(IDomainEvent domainEvent);
}