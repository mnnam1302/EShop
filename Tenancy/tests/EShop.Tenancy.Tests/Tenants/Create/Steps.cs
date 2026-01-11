using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using Reqnroll;

namespace EShop.Tenancy.Tests.Tenants.Create;

[Binding]
internal class Steps(StepContext stepContext)
{
    [Given("System User has registered tenants with following details")]
    [When("System user registers a new tenant with following details")]
    public async Task WhenSystemUserRegistersANewTenantWithFollowingDetails(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<Command.CreateTenantCommand>();
        await stepContext.CreateTenantAsync(request);
    }

    [Then("the tenant {string} has following details")]
    public async Task ThenTheTenantHasFollowingDetails(string tenantId, DataTable dataTable)
    {
        var actualTenant = await stepContext.GetTenantAsync(tenantId);
        dataTable.CompareToInstance(actualTenant);
    }
}
