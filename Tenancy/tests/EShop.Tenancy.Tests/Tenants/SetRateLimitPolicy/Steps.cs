using EShop.Tenancy.Presentation.Models;
using Reqnroll;

namespace EShop.Tenancy.Tests.Tenants.SetRateLimitPolicy;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("System User sets the rate-limit policy for tenant {string} with following rules")]
    public async Task WhenSystemUserSetsTheRateLimitPolicyForTenantWithFollowingRules(string tenantId, DataTable dataTable)
    {
        var rules = dataTable.CreateSet<RateLimitRuleRequest>().ToList();
        await stepContext.SetRateLimitPolicyAsSystemUser(tenantId, rules);
    }

    [When("a tenant user of {string} sets the rate-limit policy with following rules")]
    public async Task WhenATenantUserOfSetsTheRateLimitPolicyWithFollowingRules(string tenantId, DataTable dataTable)
    {
        var rules = dataTable.CreateSet<RateLimitRuleRequest>().ToList();
        await stepContext.SetRateLimitPolicyAsTenantUser(tenantId, rules);
    }
}
