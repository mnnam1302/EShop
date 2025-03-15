using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;

namespace EShop.Tenancy.Persistence.Repositories;

internal class TenantRepository : AggregateRepository<TenancyDbContext, Tenant, string>, ITenantRepository
{
    public TenantRepository(TenancyDbContext context) : base(context)
    {
    }
}