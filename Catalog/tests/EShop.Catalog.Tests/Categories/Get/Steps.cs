using FluentAssertions;
using Reqnroll;

namespace EShop.Catalog.Tests.Categories.Get;

[Binding]
public sealed class Steps(StepContext stepContext)
{
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

        child.ParentId.Should().Be(Guid.Parse(parent.Id));
    }
}