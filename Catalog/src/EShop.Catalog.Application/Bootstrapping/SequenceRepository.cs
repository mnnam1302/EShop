using EShop.Catalog.Application.Shared;
using EShop.Shared.Sequences;

namespace EShop.Catalog.Application.Bootstrapping;

public sealed class SequenceRepository : EntityFrameworkSequenceStore<CatalogDbContext>
{
    public SequenceRepository(CatalogDbContext dbContext) : base(dbContext)
    {
    }
}