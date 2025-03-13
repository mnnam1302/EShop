using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;

namespace EShop.Tenancy.Persistence.Repositories;

internal class FeatureRepository : RepositoryBaseDbContext<TenancyDbContext, Feature, string>, IFeatureRepository
{
    public FeatureRepository(TenancyDbContext dbContext) : base(dbContext)
    {
    }
}