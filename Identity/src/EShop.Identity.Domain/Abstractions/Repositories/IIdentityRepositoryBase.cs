using Eshop.Shared.DomainTools.Entities;

namespace EShop.Identity.Domain.Abstractions.Repositories;

/// <summary>
/// This interface is used for all entities in Identity service, but consider consider using IIdentityAggregateRepository for aggregate roots.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IIdentityRepositoryBase<TEntity, in TKey> : Eshop.Shared.DomainTools.Repositories.IRepositoryBase<TEntity, TKey>
    where TEntity : class, IEntityBase<TKey>
{
}