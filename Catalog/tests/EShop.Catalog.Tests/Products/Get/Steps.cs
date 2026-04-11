using Reqnroll;

namespace EShop.Catalog.Tests.Products.Get;

[Binding]
internal class Steps(StepContext stepContext)
{
    [Then("the product {string} has following details")]
    public async Task ThenTheProductHasFollowingDetails(string name, DataTable dataTable)
    {
        var product = await stepContext.GetProductAsync(name);
        dataTable.CompareToInstance(product);
    }

    [Then("the product {string} has the following variants")]
    public async Task ThenTheProductHasTheFollowingVariants(string name, DataTable dataTable)
    {
        var product = stepContext.LastProduct ?? await stepContext.GetProductAsync(name);

        dataTable.CompareToSet(product!.Variants);
    }
}