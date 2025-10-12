using EShop.Shared.DomainTools.Aggregates;

namespace EShop.Shared.DomainTools.Repositories;

/// <summary>
/// Defines a repository for managing an aggregate root entity.
/// </summary>
/// <typeparam name="TAggregateRoot">The type of the aggregate root entity.</typeparam>
/// <typeparam name="TKey">The type of the aggregate root entity's identifier.</typeparam>
public interface IAggregateRepository<TAggregateRoot, in TKey> : IRepository<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot<TKey>
{
}