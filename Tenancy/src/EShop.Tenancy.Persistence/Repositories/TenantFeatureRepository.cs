using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Abstractions.Repositories;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Persistence.Repositories;

internal sealed class TenantFeatureRepository : RepositoryBase<TenancyDbContext, TenantFeature, Guid>, ITenantFeatureRepository
{
    public TenantFeatureRepository(TenancyDbContext dbContext) : base(dbContext)
    {
    }
}