using Eshop.Shared.DomainTools.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Shared.DomainTools.Repositories;

public class RepositoryBaseAggregateDbContext<TDbContext, TEntity, TKey>
    : RepositoryBaseDbContext<TDbContext, TEntity, TKey>, IRepositoryBase<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class, IAggregateRoot<TKey>
{
    private readonly TDbContext _dbContext;

    public RepositoryBaseAggregateDbContext(TDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }
}