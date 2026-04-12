using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Products.Get;

internal sealed class StepContext(ApiContext apiContext)
{
    public async Task<Product?> GetProductAsync(string name)
    {
        var product = await apiContext.QueryReadModelAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IProductReadRepository>();
            return await repository.FindSingleAsync(p => p.Name == name, cancellationToken: CancellationToken.None);
        });

        return product;
    }
}