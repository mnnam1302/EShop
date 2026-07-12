using EShop.Tenancy.Application.UseCases.Tenants.GetRateLimitPolicy;
using EShop.Tenancy.Presentation.Models;
using FluentAssertions;
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

    [Then("the tenant {string} has follow rate-limit policies")]
    public async Task ThenTheTenantHasFollowRate_LimitPolicies(string tenantId, DataTable dataTable)
    {
        await stepContext.ReadRateLimitPolicyAsSystemUser(tenantId);

        stepContext.LastPolicyResult.Should().NotBeNull();
        stepContext.LastPolicyResult!.IsSuccess.Should().BeTrue();

        var response = stepContext.LastPolicyResult.Value;

        var expectedRules = dataTable.CreateSet<RateLimitRuleResponse>().ToList();
        response.Rules.Should().BeEquivalentTo(expectedRules);
    }
}
