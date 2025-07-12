using EShop.Configuration.Application.Shared;
using EShop.Shared.DomainTools.Repositories;

namespace EShop.Configuration.Application.Products;

public interface IProductRepository : IRepositoryBase<Product, Guid>
{
}

public class ProductRepository : RepositoryBaseDbContext<ConfigurationDbContext, Product, Guid>, IProductRepository
{
    public ProductRepository(ConfigurationDbContext dbContext) : base(dbContext)
    {
    }
}
