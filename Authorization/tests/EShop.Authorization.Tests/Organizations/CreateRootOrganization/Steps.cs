using Reqnroll;

namespace EShop.Authorization.Tests.Organizations.CreateRootOrganization;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("Tenancy service has provisioned a new tenant with following details")]
    public async Task WhenTenancyServiceHasProvisionedANewTenantWithFollowingDetails(DataTable dataTable)
    {
        await stepContext.PublishTenantCreatedAsync(dataTable);
    }

    [Then("User {string} retrieves organizations")]
    public async Task ThenUserRetrievesOrganizations(string username, DataTable dataTable)
    {
        var organizations = await stepContext.GetOrganizations(username);
        dataTable.CompareToSet(organizations);
    }
}
