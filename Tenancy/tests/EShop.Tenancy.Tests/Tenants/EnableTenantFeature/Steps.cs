using Reqnroll;

namespace EShop.Tenancy.Tests.Tenants.EnableTenantFeature;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("System User enables the feature {string} for tenant {string}")]
    public async Task WhenSystemUserEnablesTheFeatureForTenant(string featureId, string tenantId)
    {
        await stepContext.EnableTenantFeature(tenantId, featureId);
    }
}