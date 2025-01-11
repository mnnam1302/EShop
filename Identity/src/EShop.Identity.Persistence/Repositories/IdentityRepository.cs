using Eshop.Shared.DomainTools.Aggregates;
using Eshop.Shared.DomainTools.Repositories;
using EShop.Identity.Domain.Abstractions.Repositories;

namespace EShop.Identity.Persistence.Repositories;

public class IdentityRepository<TEntity, TKey> : AggregateRepositoryBaseDbContext<UsersDbContext, TEntity, TKey>,
    IIdentityRepository<TEntity, TKey>
    where TEntity : class, IAggregateRoot<TKey>
{
    public IdentityRepository(UsersDbContext dbContext) : base(dbContext)
    {
    }
}