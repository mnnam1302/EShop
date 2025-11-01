using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Tenancy.Tests.Tenants.Create;
using Reqnroll;

namespace EShop.Tenancy.Tests.Tenancy.Create
{
    [Binding]
    internal class Steps(StepContext stepContext)
    {
        [Given("user of group {string} logged in")]
        public void GivenUserOfGroupLoggedIn(string group)
        {
            stepContext.LoggedInGroup = group;
        }

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
}
