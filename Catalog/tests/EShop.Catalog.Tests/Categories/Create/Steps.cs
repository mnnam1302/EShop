using EShop.Catalog.Application.Categories.Create;
using EShop.Catalog.Tests.Categories;
using FluentAssertions;
using Reqnroll;

namespace EShop.Catalog.Tests.Categories.Create;

[Binding]
internal sealed class Steps(StepContext stepContext, Get.StepContext getStepContext)
{
    [Given("System User has created the following category")]
    [When("System user creates a new category")]
    public async Task WhenSystemUserCreatesANewCategory(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<CreateCategoryRequest>();
        await stepContext.CreateCategoryAsync(request);
    }

    [When("System user creates a child category with parent reference {string}")]
    public async Task WhenSystemUserCreatesAChildCategoryWithParentReference(string parentReference, DataTable dataTable)
    {
        var parent = await getStepContext.GetCategoryAsync(parentReference);

        var request = dataTable.CreateInstance<CreateCategoryRequest>();
        request.ParentId = Guid.Parse(parent.Id);

        await stepContext.CreateCategoryAsync(request);
    }
}