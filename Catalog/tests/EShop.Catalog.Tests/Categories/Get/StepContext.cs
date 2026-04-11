using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.Tests.Setup;
using EShop.Shared.DomainTools.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Categories.Get;

public sealed class StepContext(ApiContext apiContext)
{
    public async Task<Category> GetCategoryAsync(string reference)
    {
        var repository = apiContext.ServiceProvider.GetRequiredService<ICategoryReadRepository>();

        var category = await repository.FindSingleAsync(c => c.Reference == reference, cancellationToken: CancellationToken.None);
        return category.Require();
    }
}