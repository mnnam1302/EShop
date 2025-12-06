using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Persistence.Repositories;

internal class FeatureRepository : RepositoryBase<TenancyDbContext, Feature, string>, IFeatureRepository
{
    private readonly TenancyDbContext _dbContext;

    public FeatureRepository(TenancyDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EntityState> GetEntityStateAsync(Feature feature, CancellationToken cancellationToken = default)
    {
        var state = _dbContext.Entry(feature).State;
        return Task.FromResult(state);
    }
}