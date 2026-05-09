using EShop.Catalog.Application.Products.ChangeVariantPrice;
using Reqnroll;

namespace EShop.Catalog.Tests.Products.ChangeVariantPrice;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    [When("System user changes the price of variant {string} of product {string}")]
    public async Task WhenSystemUserChangesThePriceOfVariantOfProduct(string sku, string productName, DataTable dataTable)
    {
        var product = await getStepContext.GetProductAsync(productName)
            ?? throw new InvalidOperationException($"Product '{productName}' not found.");

        var variant = product.Variants.First(v => v.Sku == sku);
        var row = dataTable.Rows[0];

        var request = new ChangeVariantPriceRequest
        {
            Price = decimal.Parse(row["Price"]),
            DiscountPrice = decimal.Parse(row["DiscountPrice"])
        };

        await stepContext.ChangeVariantPriceAsync(product.Id, variant.Id, request);
    }
}
