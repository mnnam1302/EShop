using EShop.Configuration.Application.Shared;
using EShop.Shared.Sequences;

namespace EShop.Configuration.Application.Boostrapping;

internal class SequenceRepository : EntityFrameworkSequenceStore<ConfigurationDbContext>
{
    public SequenceRepository(ILogger logger, ConfigurationDbContext dbContext) : base(logger, dbContext)
    {
    }
}
