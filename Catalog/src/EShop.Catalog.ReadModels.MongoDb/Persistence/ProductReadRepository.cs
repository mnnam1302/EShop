using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Catalog.ReadModels.MongoDb.Persistence;

public sealed class ProductReadRepository : RepositoryBase<CatalogReadDbContext, Product, string>, IProductReadRepository
{
    public ProductReadRepository(CatalogReadDbContext dbContext)
        : base(dbContext)
    {
    }
}
