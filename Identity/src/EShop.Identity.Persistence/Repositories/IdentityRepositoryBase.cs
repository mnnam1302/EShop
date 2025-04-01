using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Repositories;
using EShop.Identity.Domain.Repositories;

namespace EShop.Identity.Persistence.Repositories;

public class IdentityRepositoryBase<TEntity, TKey>
    : RepositoryBaseDbContext<UsersDbContext, TEntity, TKey>, IIdentityRepositoryBase<TEntity, TKey>
    where TEntity : class, IEntityBase<TKey>
{
    public IdentityRepositoryBase(UsersDbContext dbContext) : base(dbContext)
    {
    }
}