using EShop.Catalog.Application.Shared;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Catalog.Application.Agencies;

public interface IAgencyRepository : IRepositoryBase<Agency, Guid>;

public sealed class AgencyRepository : RepositoryBase<CatalogDbContext, Agency, Guid>, IAgencyRepository
{
    public AgencyRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }
}
