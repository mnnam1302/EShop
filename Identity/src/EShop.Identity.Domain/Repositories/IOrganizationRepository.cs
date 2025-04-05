using EShop.Identity.Domain.Entities;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Identity.Domain.Repositories;

public interface IOrganizationRepository : IAggregateRepository<Organization, string>
{
}