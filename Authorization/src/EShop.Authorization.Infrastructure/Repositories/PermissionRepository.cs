using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Authorization.Infrastructure.Repositories;

public class PermissionRepository : RepositoryBase<AuthorizationDbContext, Permission, string>, IPermissionRepository
{
    public PermissionRepository(AuthorizationDbContext dbContext) : base(dbContext)
    {
    }
}
