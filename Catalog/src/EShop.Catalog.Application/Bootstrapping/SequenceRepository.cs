using EShop.Catalog.Application.Shared;
using EShop.Shared.Sequences;

namespace EShop.Catalog.Application.Boostrapping;

public sealed class SequenceRepository : EntityFrameworkSequenceStore<CatalogDbContext>
{
    public SequenceRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }
}