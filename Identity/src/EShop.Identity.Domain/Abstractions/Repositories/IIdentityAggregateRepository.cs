using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Identity.Domain.Abstractions.Repositories;

/// <summary>
/// TAggregate is used IAggregateRoot because the Identity service  
/// can have multiple aggregate roots.  
/// </summary>
/// <typeparam name="TAggregate">Represents the aggregate root entity for consistency.</typeparam>
/// <typeparam name="TKey">Represents the ID of the aggregate root entity.</typeparam>
public interface IIdentityAggregateRepository<TAggregate, TKey>
    : IAggregateRepository<TAggregate, TKey>
    where TAggregate : class, IAggregateRoot<TKey>
{
}