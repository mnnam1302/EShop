using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.Repositories;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Authorization.Infrastructure.Repositories;

public sealed class OrganizationRepository : AggregateRepository<AuthorizationDbContext, Organization, string>, IOrganizationRepository
{
    public OrganizationRepository(AuthorizationDbContext dbContext) : base(dbContext)
    {
    }
}
