using EShop.Shared.DomainTools.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EShop.Shared.DomainTools.Repositories;

public abstract class EFRepository<TDbContext, TEntity, TKey> : IRepository<TEntity, TKey>, IDisposable
    where TDbContext : DbContext
    where TEntity : class, IEntityBase<TKey>
{
    private readonly TDbContext _dbContext;

    protected EFRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected TDbContext DbContext => _dbContext;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dbContext.Dispose();
        }
    }

    public async Task<TEntity?> FindByIdAsync(
        TKey id,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var entity = await FindByCondition(x => x.Id!.Equals(id), trackChanges, includeProperties)
            .FirstOrDefaultAsync(cancellationToken);

        return entity;
    }

    public async Task<TEntity?> FindSingleAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindByCondition(predicate, trackChanges, includeProperties)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ICollection<TEntity>> FindByConditionAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool trackChanges = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindByCondition(predicate, trackChanges, includeProperties)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<TEntity> FindByCondition(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool trackChanges = false,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var items = FindAll(trackChanges, includeProperties);

        if (includeProperties != null)
        {
            foreach (var includeProperty in includeProperties)
            {
                items = items.Include(includeProperty);
            }
        }

        if (predicate != null)
        {
            items = items.Where(predicate);
        }

        return items;
    }

    public IQueryable<TEntity> FindAll(bool trackChanges = false, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        // Important to use AsNoTracking to improve performance - Always include AsNoTracking for Query Side
        IQueryable<TEntity> items = trackChanges
            ? _dbContext.Set<TEntity>()
            : _dbContext.Set<TEntity>().AsNoTracking();

        if (includeProperties != null)
        {
            foreach (var includeProperty in includeProperties)
            {
                items = items.Include(includeProperty);
            }
        }

        return items;
    }

    public void Add(TEntity entity)
    {
        _dbContext.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }

    public void DeleteMultiple(ICollection<TEntity> entities)
    {
        _dbContext.Set<TEntity>().RemoveRange(entities);
    }
}