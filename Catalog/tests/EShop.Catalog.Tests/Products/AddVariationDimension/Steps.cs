using EShop.Catalog.Application.Products.AddVariationDimension;
using Reqnroll;

namespace EShop.Catalog.Tests.Products.AddVariationDimension;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    [Given("System user has added a variation dimension to the product {string}")]
    [When("System user adds a variation dimension to the product {string}")]
    public async Task SystemUserAddsAVariationDimensionToTheProduct(string productName, DataTable dataTable)
    {
        var product = await getStepContext.GetProductAsync(productName)
            ?? throw new InvalidOperationException($"Product '{productName}' not found.");

        var row = dataTable.Rows[0];
        var request = new AddVariationDimensionRequest
        {
            Name = row["Name"],
            DisplayName = row["DisplayName"],
            Values = row["Values"].Split(',', StringSplitOptions.RemoveEmptyEntries),
            DisplayStyle = row["DisplayStyle"]
        };

        await stepContext.AddVariationDimensionAsync(Guid.Parse(product.Id), request);
    }

    [Then("the product {string} has the following variation dimensions")]
    public async Task ThenTheProductHasTheFollowingVariationDimensions(string productName, DataTable dataTable)
    {
        var product = await getStepContext.GetProductAsync(productName);
        dataTable.CompareToSet(product!.VariationDimensions);
    }
}
