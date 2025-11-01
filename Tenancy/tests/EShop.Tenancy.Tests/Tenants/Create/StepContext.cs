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
        var systemUser = UserData.GetSystemUser(LoggedInGroup);
        await apiContext.PostAsync($"{BaseUrl}", request, systemUser);
    }

    internal async Task<TenantDetailsResponse> GetTenantAsync(string tenantId)
    {
        var systemUser = UserData.GetSystemUser(LoggedInGroup);
        var result = await apiContext.GetAsync<TenantDetailsResponse>($"{BaseUrl}/{tenantId}", systemUser);

        return result.Value;
    }
}
