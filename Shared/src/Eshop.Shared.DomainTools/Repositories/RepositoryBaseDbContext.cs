using Eshop.Shared.DomainTools.Aggregates;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Eshop.Shared.DomainTools.Repositories;

public class RepositoryBaseDbContext<TDbContext, TEntity, TKey> : IRepositoryBase<TEntity, TKey>, IDisposable
    where TDbContext : DbContext
    where TEntity : class, IAggregateRoot<TKey>
{
    private readonly TDbContext _dbContext;

    public RepositoryBaseDbContext(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public async Task<TEntity?> FindByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(x => x.Id!.Equals(id), includeProperties)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> FindSingleAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(predicate, includeProperties)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ICollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(predicate, includeProperties)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<TEntity> FindAll(
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        // Important to use AsNoTracking to improve performance - Always include AsNoTracking for Query Side
        IQueryable<TEntity> items = _dbContext.Set<TEntity>().AsNoTracking();

        if (includeProperties != null)
        {
            foreach (var includeProperty in includeProperties)
            {
                items = items.Include(includeProperty);
            }
        }

        if (predicate != null)
            items.Where(predicate);

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

    public void Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }

    public void RemoveMultiple(List<TEntity> entities)
    {
        _dbContext.Set<TEntity>().RemoveRange(entities);
    }
}