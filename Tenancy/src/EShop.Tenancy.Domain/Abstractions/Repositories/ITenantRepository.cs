using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Domain.Abstractions.Repositories;

public interface ITenantRepository : IAggregateRepository<Tenant, string>
{
}