using Reqnroll;

namespace EShop.Catalog.Tests.Products.Unpublish;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    [When("System user unpublishes the product {string}")]
    public async Task WhenSystemUserUnpublishesTheProduct(string productName)
    {
        var product = await getStepContext.GetProductAsync(productName)
            ?? throw new InvalidOperationException($"Product '{productName}' not found.");

        await stepContext.UnpublishProductAsync(Guid.Parse(product.Id));
    }
}
