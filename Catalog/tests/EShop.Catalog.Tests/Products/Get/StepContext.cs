using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Products.Get;

internal sealed class StepContext(ApiContext apiContext)
{
    public Product? LastProduct { get; private set; }

    public async Task<Category> GetCategoryAsync(string reference)
    {
        var repository = apiContext.ServiceProvider.GetRequiredService<ICategoryReadRepository>();
        return await repository.FindSingleAsync(c => c.Reference == reference, cancellationToken: CancellationToken.None)
            ?? throw new InvalidOperationException($"Category with reference '{reference}' not found.");
    }

    public async Task<Product?> GetProductAsync(string name)
    {
        var repository = apiContext.ServiceProvider.GetRequiredService<IProductReadRepository>();
        LastProduct = await repository.FindSingleAsync(p => p.Name == name, cancellationToken: CancellationToken.None);

        return LastProduct;
    }
}