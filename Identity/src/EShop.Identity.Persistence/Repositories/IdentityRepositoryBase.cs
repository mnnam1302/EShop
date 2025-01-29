using EShop.Shared.DomainTools.Entities;
using EShop.Identity.Domain.Abstractions.Repositories;

namespace EShop.Identity.Persistence.Repositories;

public class IdentityRepositoryBase<TEntity, TKey> 
    : EShop.Shared.DomainTools.Repositories.RepositoryBaseDbContext<UsersDbContext, TEntity, TKey>, IIdentityRepositoryBase<TEntity, TKey>
    where TEntity : class, IEntityBase<TKey>
{
    public IdentityRepositoryBase(UsersDbContext dbContext) : base(dbContext)
    {
    }
}