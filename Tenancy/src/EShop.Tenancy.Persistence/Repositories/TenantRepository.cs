using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Abstractions.Repositories;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Persistence.Repositories;

internal class TenantRepository : AggregateRepository<TenancyDbContext, Tenant, string>, ITenantRepository
{
    public TenantRepository(TenancyDbContext context) : base(context)
    {
    }
}