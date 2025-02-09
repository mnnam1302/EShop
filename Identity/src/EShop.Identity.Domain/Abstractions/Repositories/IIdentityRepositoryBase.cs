using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Identity.Domain.Abstractions.Repositories;

/// <summary>
/// Defines a repository for all entities in the Identity service.  
/// However, consider using <see cref="IIdentityAggregateRepository{TAggregate, TKey}"/> for aggregate roots  
/// to maintain proper Domain-Driven Design (DDD) principles.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
public interface IIdentityRepositoryBase<TEntity, in TKey> : IRepositoryBase<TEntity, TKey>
    where TEntity : class, IEntityBase<TKey>
{
}