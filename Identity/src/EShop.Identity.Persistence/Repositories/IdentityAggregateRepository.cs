using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Identity.Persistence.Repositories;

public class IdentityAggregateRepository<TEntity, TKey> : AggregateRepository<UsersDbContext, TEntity, TKey>,
    IIdentityAggregateRepository<TEntity, TKey>
    where TEntity : class, IAggregateRoot<TKey>
{
    public IdentityAggregateRepository(UsersDbContext dbContext) : base(dbContext)
    {
    }
}