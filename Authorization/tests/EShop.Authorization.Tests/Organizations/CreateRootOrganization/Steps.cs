using Reqnroll;

namespace EShop.Authorization.Tests.Organizations.CreateRoot;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("Tenancy service has provisioned a new tenant with following details")]
    public async Task WhenTenancyServiceHasProvisionedANewTenantWithFollowingDetails(DataTable dataTable)
    {
        await stepContext.PublishTenantCreatedAsync(dataTable);
    }

    [Then("there are following organizations created")]
    public void ThenThereAreFollowingOrganizationsCreated(DataTable dataTable)
    {
        throw new PendingStepException();
    }
}
