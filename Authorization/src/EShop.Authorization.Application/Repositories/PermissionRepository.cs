using EShop.Authorization.Application.Shared;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Authorization.Application.Repositories;

public class PermissionRepository : EFRepository<AuthorizationDbContext, Permission, string>, IPermissionRepository
{
    public PermissionRepository(AuthorizationDbContext dbContext) : base(dbContext)
    {
    }
}
