using EShop.Catalog.Application.Shared;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Catalog.Application.Agencies;

public interface IAgencyRepository : IRepository<Agency, Guid>;

public sealed class AgencyRepository : RepositoryBaseDbContext<CatalogDbContext, Agency, Guid>, IAgencyRepository
{
    public AgencyRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }
}
