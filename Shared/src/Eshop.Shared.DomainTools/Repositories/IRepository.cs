using EShop.Shared.DomainTools.Entities;
using System.Linq.Expressions;

namespace EShop.Shared.DomainTools.Repositories;

/// <summary>
/// Defines a generic repository interface for performing CRUD operations and querying entities.
/// </summary>
/// <remarks>This interface provides methods for retrieving, adding, updating, and deleting entities, as well as
/// querying entities with optional filtering, tracking, and inclusion of related properties.</remarks>
/// <typeparam name="TEntity">The type of the entity managed by the repository. Must be a class implementing <see cref="IEntityBase{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The type of the unique identifier for the entity.</typeparam>
public interface IRepository<TEntity, in TKey> where TEntity : class, IEntityBase<TKey>
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