using EShop.Catalog.Application.Categories.Update;
using Reqnroll;

namespace EShop.Catalog.Tests.Categories.Update;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("System user updates category with reference {string}")]
    public async Task WhenSystemUserUpdatesCategoryWithReference(string reference, DataTable dataTable)
    {
        var request = dataTable.CreateInstance<UpdateCategoryRequest>();
        await stepContext.UpdateCategoryAsync(reference, request);
    }
}
