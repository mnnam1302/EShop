using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Identity.Persistence.Repositories;

public class OrganizationRepository : AggregateRepository<UsersDbContext, Organization, string>, IOrganizationRepository
{
    public OrganizationRepository(UsersDbContext dbContext) : base(dbContext)
    {
    }
}