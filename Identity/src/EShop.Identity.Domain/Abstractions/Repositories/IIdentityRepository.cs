using Eshop.Shared.DomainTools.Aggregates;

namespace EShop.Identity.Domain.Abstractions.Repositories;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity">Represent aggregate root entity to consistency</typeparam>
/// <typeparam name="TKey">Represent aggregate root entity's id</typeparam>
public interface IIdentityRepository<TEntity, TKey> 
    : Eshop.Shared.DomainTools.Repositories.IRepositoryBase<TEntity, TKey>
    where TEntity : class, IAggregateRoot<TKey>
{
}