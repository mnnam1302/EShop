using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Catalog.ReadModels.MongoDb.Persistence;

public sealed class CategoryReadRepository : RepositoryBase<CatalogReadDbContext, Category, string>, ICategoryReadRepository
{
    public CategoryReadRepository(CatalogReadDbContext dbContext)
        : base(dbContext)
    {
    }
}
