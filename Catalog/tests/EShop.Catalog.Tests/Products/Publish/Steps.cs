using Reqnroll;

namespace EShop.Catalog.Tests.Products.Publish;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    [When("System user publishes the product {string}")]
    public async Task WhenSystemUserPublishesTheProduct(string productName)
    {
        var product = await getStepContext.GetProductAsync(productName)
            ?? throw new InvalidOperationException($"Product '{productName}' not found.");

        await stepContext.PublishProductAsync(Guid.Parse(product.Id));
    }

    [Given("the product {string} is published")]
    public async Task GivenTheProductIsPublished(string productName)
    {
        var product = await getStepContext.GetProductAsync(productName)
            ?? throw new InvalidOperationException($"Product '{productName}' not found.");

        await stepContext.PublishProductAsync(Guid.Parse(product.Id));
    }
}
