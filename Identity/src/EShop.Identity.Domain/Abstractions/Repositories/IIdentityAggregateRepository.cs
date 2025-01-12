using Eshop.Shared.DomainTools.Aggregates;

namespace EShop.Identity.Domain.Abstractions.Repositories;

/// <summary>
/// Why here is TAggregate without Organization. Because Identity service can have multiple aggregate roots.
/// </summary>
/// <typeparam name="TAggregate">Represent aggregate root entity to consistency</typeparam>
/// <typeparam name="TKey">Represent aggregate root entity's id</typeparam>
public interface IIdentityAggregateRepository<TAggregate, TKey> 
    : Eshop.Shared.DomainTools.Repositories.IAggregateRepository<TAggregate, TKey>
    where TAggregate : class, IAggregateRoot<TKey>
{
}