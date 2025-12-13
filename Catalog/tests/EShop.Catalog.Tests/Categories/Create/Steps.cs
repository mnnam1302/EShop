using EShop.Catalog.Application.Categories.Create;
using Reqnroll;

namespace EShop.Catalog.Tests.Categories.Create;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("System user creates a new category")]
    public async Task WhenSystemUserCreatesANewCategory(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<CreateCategoryRequest>();
        await stepContext.CreateCategoryAsync(request);
    }

    [Then("the category {string} has following details")]
    public void ThenTheCategoryHasFollowingDetails(string p0, DataTable dataTable)
    {
        throw new PendingStepException();
    }
}