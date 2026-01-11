using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;

namespace EShop.Tenancy.Persistence.Repositories;

internal sealed class TenantFeatureRepository : RepositoryBase<TenancyDbContext, TenantFeature, Guid>, ITenantFeatureRepository
{
    public TenantFeatureRepository(TenancyDbContext dbContext) : base(dbContext)
    {
    }
}