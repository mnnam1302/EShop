using EShop.Shared.DomainTools.Entities;
using System.Linq.Expressions;

namespace EShop.Shared.DomainTools.Repositories;

public interface IRepositoryBase<TEntity, in TKey> where TEntity : class, IEntityBase<TKey>
{
    Task<TEntity?> FindByIdAsync(
        TKey id,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties);

    Task<TEntity?> FindSingleAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties);

    Task<ICollection<TEntity>> FindByConditionAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties);

    IQueryable<TEntity> FindByCondition(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool trackChanges = false,
        params Expression<Func<TEntity, object>>[] includeProperties);

    IQueryable<TEntity> FindAll(
        bool trackChanges = false,
        params Expression<Func<TEntity, object>>[] includeProperties);

    void Add(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    void DeleteMultiple(ICollection<TEntity> entities);
}