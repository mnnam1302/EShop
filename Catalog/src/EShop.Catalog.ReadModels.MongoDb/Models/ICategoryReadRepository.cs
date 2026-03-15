using EShop.Shared.DomainTools.Repositories;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

public interface ICategoryReadRepository : IRepositoryBase<Category, string>
{
}
