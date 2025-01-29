using System.Linq.Expressions;

namespace EShop.Shared.DomainTools.Repositories;

/// <summary>
/// This interface is used to define a repository for an entity.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IRepositoryBase<TEntity, in TKey>
    where TEntity : class
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