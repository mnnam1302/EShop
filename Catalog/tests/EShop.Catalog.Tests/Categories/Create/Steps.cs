using EShop.Catalog.Application.Categories.Create;
using EShop.Catalog.Tests.Categories;
using FluentAssertions;
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

    [When("System user creates a child category with parent reference {string}")]
    public async Task WhenSystemUserCreatesAChildCategoryWithParentReference(string parentReference, DataTable dataTable)
    {
        var request = dataTable.CreateInstance<CreateCategoryRequest>();
        await stepContext.CreateChildCategoryAsync(request, parentReference);
    }

    [Then("the category {string} has following details")]
    public async Task ThenTheCategoryHasFollowingDetails(string reference, DataTable dataTable)
    {
        var category = await stepContext.GetCategoryAsync(reference);
        dataTable.CompareToInstance(category);
    }

    [Then("the category {string} has parent {string}")]
    public async Task ThenTheCategoryHasParent(string childReference, string parentReference)
    {
        var child = await stepContext.GetCategoryAsync(childReference);
        var parent = await stepContext.GetCategoryAsync(parentReference);

        child.ParentId.Should().Be(parent.DocumentId);
    }
}