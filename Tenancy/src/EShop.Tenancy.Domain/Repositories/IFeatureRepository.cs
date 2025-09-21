using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Domain.Repositories;

public interface IFeatureRepository : IRepository<Feature, string>
{
    Task<EntityState> GetEntityStateAsync(Feature feature, CancellationToken cancellationToken = default);
}