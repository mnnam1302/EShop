using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Aggregates;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Persistence.Repositories;

public class TenantRepository : AggregateRepository<TenancyDbContext, Tenant, string>, ITenantRepository
{
    public TenantRepository(TenancyDbContext context) : base(context)
    {
    }
}