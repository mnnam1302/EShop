using EShop.Shared.DomainTools.Repositories;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Domain.Repositories;

public interface ITenantFeatureRepository : IRepositoryBase<TenantFeature, Guid>
{
}