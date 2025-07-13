using EShop.Configuration.Application.Shared;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Configuration.Application.Agencies;

public interface IAgencyRepository : IRepositoryBase<Agency, string>
{
}

public class AgencyRepository : RepositoryBaseDbContext<ConfigurationDbContext, Agency, string>, IAgencyRepository
{
    public AgencyRepository(ConfigurationDbContext dbContext) : base(dbContext)
    {
    }
}
