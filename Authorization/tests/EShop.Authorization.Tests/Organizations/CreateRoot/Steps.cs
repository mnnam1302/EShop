using Reqnroll;

namespace EShop.Authorization.Tests.Organizations.CreateRoot;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [Given("Tenancy service has provisied a new tenant with following details")]
    public async Task GivenTenancyServiceHasProvisiedANewTenantWithFollowingDetails(DataTable dataTable)
    {
        await stepContext.PublishTenantCreatedAsync(dataTable);
    }
}
