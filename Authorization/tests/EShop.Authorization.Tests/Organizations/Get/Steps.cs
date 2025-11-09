using Reqnroll;

namespace EShop.Authorization.Tests.Organizations.Get;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [Then("organizaion {string} has following details")]
    public void ThenOrganizaionHasFollowingDetails(string organizationId, DataTable dataTable)
    {
        return;
    }

    [Then("user {string} retrieves organizations")]
    public async Task ThenUserRetrievesOrganizations(string username, DataTable dataTable)
    {
        var organizations = await stepContext.GetOrganizations(username);
        dataTable.CompareToSet(organizations);
    }
}
