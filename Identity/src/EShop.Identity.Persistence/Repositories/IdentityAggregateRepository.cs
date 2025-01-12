using Eshop.Shared.DomainTools.Aggregates;
using Eshop.Shared.DomainTools.Repositories;
using EShop.Identity.Domain.Abstractions.Repositories;

namespace EShop.Identity.Persistence.Repositories;

public class IdentityAggregateRepository<TEntity, TKey> : AggregateRepository<UsersDbContext, TEntity, TKey>,
    IIdentityAggregateRepository<TEntity, TKey>
    where TEntity : class, IAggregateRoot<TKey>
{
    public IdentityAggregateRepository(UsersDbContext dbContext) : base(dbContext)
    {
    }
}