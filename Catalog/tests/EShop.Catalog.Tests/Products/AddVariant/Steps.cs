using EShop.Catalog.Application.Products.AddVariant;
using Reqnroll;

namespace EShop.Catalog.Tests.Products.AddVariant;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    private static readonly HashSet<string> KnownVariantColumns =
        new(StringComparer.OrdinalIgnoreCase) { "Name", "Sku", "Price", "DiscountPrice" };

    [When("System user adds a variant to the product {string}")]
    public async Task WhenSystemUserAddsAVariantToTheProduct(string productName, DataTable dataTable)
    {
        var product = await getStepContext.GetProductAsync(productName)
            ?? throw new InvalidOperationException($"Product '{productName}' not found.");

        var row = dataTable.Rows[0];
        var dimensionValues = row.Keys
            .Where(k => !KnownVariantColumns.Contains(k))
            .Select(k => new AddVariantDimensionValueRequest { Name = k, Value = row[k] })
            .ToList();

        var request = new AddVariantRequest
        {
            Name = row["Name"],
            Sku = row["Sku"],
            Price = decimal.Parse(row["Price"]),
            DiscountPrice = decimal.Parse(row["DiscountPrice"]),
            DimensionValues = dimensionValues
        };

        await stepContext.AddVariantAsync(product.Id, request);
    }
}
