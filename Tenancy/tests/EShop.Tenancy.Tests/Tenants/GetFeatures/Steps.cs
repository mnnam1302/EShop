using Reqnroll;

namespace EShop.Tenancy.Tests.Tenants.GetFeatures;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [Then("the tenant {string} has following features")]
    public async Task ThenTheTenantHasFollowingFeatures(string tenantId, DataTable dataTable)
    {
        var tenantFeatures = await stepContext.GetTenantFeaturesAsync(tenantId);
        dataTable.CompareToSet(tenantFeatures);
    }
}