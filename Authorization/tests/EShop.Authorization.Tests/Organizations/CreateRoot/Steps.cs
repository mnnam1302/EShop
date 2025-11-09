using Reqnroll;

namespace EShop.Authorization.Tests.Organizations.CreateRoot;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("Tenancy service provision a new tenant with following details")]
    public async Task GivenTenancyServiceHasProvisionedANewTenantWithFollowingDetails(DataTable dataTable)
    {
        await stepContext.PublishTenantCreatedAsync(dataTable);
    }
}
