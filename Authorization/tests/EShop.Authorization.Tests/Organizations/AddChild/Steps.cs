using EShop.Authorization.API.Models;
using Reqnroll;

namespace EShop.Authorization.Tests.Organizations.AddChild;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("user {string} adds a child organization under the organization {string} with the following details")]
    public async Task WhenUserAddsAChildOrganizationUnderTheOrganizationWithTheFollowingDetails(string username, string parentOrganizationId, DataTable dataTable)
    {
        var request = dataTable.CreateInstance<AddChildOrganizationRequest>();
        await stepContext.AddChildOrganizationAsync(request, parentOrganizationId, username);
    }
}
