using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;
using EShop.Tenancy.Tests.Setups;

namespace EShop.Tenancy.Tests.Tenants.Create;

internal class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/tenants";

    public string LoggedInGroup { get; internal set; } = string.Empty;

    internal async Task CreateTenantAsync(Command.CreateTenantCommand request)
    {
        try
        {
            var operationalUser = new UserData("TEST_ADMIN", "TEST_ADMIN", LoggedInGroup, LoggedInGroup == UserData.EShopSupportGroup);
            await apiContext.PostAsync($"{BaseUrl}", request, operationalUser);
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }

    internal async Task<TenantDetailsResponse> GetTenantAsync(string tenantId, string? operationalUsername = null)
    {
        var operationUser = apiContext.GetUserByUsername(operationalUsername);

        var result = await apiContext.GetAsync<TenantDetailsResponse>($"{BaseUrl}/{tenantId}", operationUser);
        return result.Value;
    }
}
