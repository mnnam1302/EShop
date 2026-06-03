using EShop.Shared.Authentication;
using EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;
using EShop.Tenancy.Domain.Commands;
using EShop.Tenancy.Tests.Setups;

namespace EShop.Tenancy.Tests.Tenants.Create;

internal class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/tenants";

    internal async Task CreateTenantAsync(CreateTenantCommand request)
    {
        var systemUser = UserData.GetSystemUser(request.Id);
        await apiContext.PostAsync($"{BaseUrl}", request, systemUser);
    }

    internal async Task<TenantDetailsResponse> GetTenantAsync(string tenantId)
    {
        var systemUser = UserData.GetSystemUser(tenantId);
        var result = await apiContext.GetAsync<TenantDetailsResponse>($"{BaseUrl}/{tenantId}", systemUser);

        return result.Value;
    }
}
