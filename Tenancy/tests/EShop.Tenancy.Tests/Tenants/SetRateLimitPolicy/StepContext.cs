using EShop.Shared.Authentication;
using EShop.Tenancy.Presentation.Models;
using EShop.Tenancy.Tests.Setups;

namespace EShop.Tenancy.Tests.Tenants.SetRateLimitPolicy;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/tenants";

    public async Task SetRateLimitPolicyAsSystemUser(string tenantId, IReadOnlyList<RateLimitRuleRequest> rules)
    {
        try
        {
            var systemUser = UserData.GetSystemUser(tenantId);
            await SetRateLimitPolicy(tenantId, rules, systemUser);
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }

    public async Task SetRateLimitPolicyAsTenantUser(string tenantId, IReadOnlyList<RateLimitRuleRequest> rules)
    {
        var tenantUser = new UserData("tenant-user", "tenant-user", tenantId);
        await SetRateLimitPolicy(tenantId, rules, tenantUser);
    }

    private async Task SetRateLimitPolicy(string tenantId, IReadOnlyList<RateLimitRuleRequest> rules, UserData user)
    {
        try
        {
            var request = new SetRateLimitPolicyRequest { Rules = rules };
            await apiContext.PutAsync($"{BaseUrl}/{tenantId}/rate-limit-policy", request, user);
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }
}
