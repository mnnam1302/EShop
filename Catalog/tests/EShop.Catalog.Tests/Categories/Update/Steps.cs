using EShop.Catalog.Application.Categories.Update;
using Reqnroll;

namespace EShop.Catalog.Tests.Categories.Update;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    [When("System user updates category with reference {string}")]
    public async Task WhenSystemUserUpdatesCategoryWithReference(string reference, DataTable dataTable)
    {
        var request = dataTable.CreateInstance<UpdateCategoryRequest>();

        var category = await getStepContext.GetCategoryAsync(reference);
        await stepContext.UpdateCategoryAsync(category.Id, request);
    }
}
