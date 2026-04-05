using EShop.Shared.DomainTools.Repositories;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

public interface IProductReadRepository : IRepositoryBase<Product, string>
{
}
