using Eshop.Shared.DomainTools.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Shared.DomainTools.Repositories;

public class AggregateRepositoryBaseDbContext<TDbContext, TEntity, TKey>
    : RepositoryBaseDbContext<TDbContext, TEntity, TKey>, IRepositoryBase<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class, IAggregateRoot<TKey>
{
    public AggregateRepositoryBaseDbContext(TDbContext dbContext)
        : base(dbContext)
    {
    }
}