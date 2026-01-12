using EShop.Shared.Authentication;
using EShop.Tenancy.Tests.Setups;

namespace EShop.Tenancy.Tests.Tenants.EnableTenantFeature;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/tenants";

    public async Task EnableTenantFeature(string tenantId, string featureId)
    {
        try
        {
            var systemUser = UserData.GetSystemUser(tenantId);
            await apiContext.PatchAsync($"{BaseUrl}/{tenantId}/features/{featureId}/enable", systemUser);
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }
}