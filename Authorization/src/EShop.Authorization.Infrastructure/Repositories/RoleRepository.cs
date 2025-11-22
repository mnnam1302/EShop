using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Authorization.Infrastructure.Repositories;

public sealed class RoleRepository : RepositoryBase<AuthorizationDbContext, Role, Guid>, IRoleRepository
{
    public RoleRepository(AuthorizationDbContext dbContext) : base(dbContext)
    {
    }
}
