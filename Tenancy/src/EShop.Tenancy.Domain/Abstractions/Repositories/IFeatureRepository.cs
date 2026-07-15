using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Domain.Abstractions.Repositories;

public interface IFeatureRepository : IRepositoryBase<Feature, string>
{
    Task<EntityState> GetEntityStateAsync(Feature feature, CancellationToken cancellationToken = default);
}