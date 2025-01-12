using Eshop.Shared.DomainTools.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Eshop.Shared.DomainTools.Repositories;

public class AggregateRepository<TDbContext, TAggregateRoot, TKey>
    : RepositoryBaseDbContext<TDbContext, TAggregateRoot, TKey>, IAggregateRepository<TAggregateRoot, TKey>
    where TDbContext : DbContext
    where TAggregateRoot : class, IAggregateRoot<TKey>
{
    public AggregateRepository(TDbContext dbContext)
        : base(dbContext)
    {
    }
}