using EShop.Catalog.Application.Products.ChangeVariantPrice;
using Reqnroll;

namespace EShop.Catalog.Tests.Products.ChangeVariantPrice;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    [When("System user changes the price of the default variant of product {string}")]
    public async Task WhenSystemUserChangesThePriceOfTheDefaultVariantOfProduct(string productName, DataTable dataTable)
    {
        var product = await getStepContext.GetProductAsync(productName)
            ?? throw new InvalidOperationException($"Product '{productName}' not found.");

        var defaultVariant = product.Variants.First(v => v.IsDefault);
        var row = dataTable.Rows[0];

        var request = new ChangeVariantPriceRequest
        {
            Price = decimal.Parse(row["Price"]),
            DiscountPrice = decimal.Parse(row["DiscountPrice"])
        };

        await stepContext.ChangeVariantPriceAsync(product.Id, defaultVariant.Id, request);
    }
}
