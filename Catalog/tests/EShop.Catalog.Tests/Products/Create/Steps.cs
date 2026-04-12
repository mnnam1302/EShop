using EShop.Catalog.Application.Products.Create;
using Reqnroll;

namespace EShop.Catalog.Tests.Products.Create;

[Binding]
internal sealed class Steps(StepContext stepContext, Categories.Get.StepContext categoriesStepContext)
{
    [Given("System User has created the following product under category {string}")]
    [When("System user creates a new product under category {string}")]
    public async Task WhenSystemUserCreatesANewProductUnderCategory(string categoryReference, DataTable dataTable)
    {
        var category = await categoriesStepContext.GetCategoryAsync(categoryReference);

        var row = dataTable.Rows[0];
        var request = new CreateProductRequest
        {
            Name = row["Name"],
            Description = row["Description"],
            Price = decimal.Parse(row["Price"]),
            DiscountPrice = decimal.Parse(row["DiscountPrice"]),
            Slug = row["Slug"],
            Tags = row["Tags"].Split(',', StringSplitOptions.RemoveEmptyEntries),
            Images = row["Images"].Split(',', StringSplitOptions.RemoveEmptyEntries),
            CategoryId = Guid.Parse(category.Id)
        };

        await stepContext.CreateProductAsync(request);
    }
}