using Identity.Domain.Abstractions.Entities;
using Identity.Domain.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

namespace Identity.Persistence.Repositories;

public class RepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey>, IDisposable
    where TEntity : class, IEntityBase<TKey>
{
    private readonly UserDbContext _dbContext;

    public RepositoryBase(UserDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> items = _dbContext.Set<TEntity>().AsNoTracking();
        if (includeProperties != null)
            foreach (var includeProperty in includeProperties)
                items = items.Include(includeProperty);

        if (predicate is not null)
            items = items.Where(predicate);

        return items;
    }

    public async Task<ICollection<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(predicate, includeProperties).ToListAsync();
    }

    public async Task<TEntity> FindByIdAsync(TKey id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(null, includeProperties)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }

    public async Task<TEntity> FindSingleAsync(Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        return await FindAll(predicate, includeProperties)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
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