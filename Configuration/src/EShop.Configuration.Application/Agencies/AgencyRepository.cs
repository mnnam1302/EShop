using EShop.Configuration.Application.Shared;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Configuration.Application.Agencies;

public interface IAgencyRepository : IRepositoryBase<Agency, Guid>;

public sealed class AgencyRepository : RepositoryBaseDbContext<ConfigurationDbContext, Agency, Guid>, IAgencyRepository
{
    public AgencyRepository(ConfigurationDbContext dbContext) : base(dbContext)
    {
    }
}
