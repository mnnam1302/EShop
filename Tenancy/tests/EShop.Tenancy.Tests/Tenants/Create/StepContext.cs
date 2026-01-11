using EShop.Shared.Authentication;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Tests.Setups;
using EShop.Testing.JsonApiApplication;

namespace EShop.Tenancy.Tests.Tenants.Create;

internal class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/tenants";

    internal async Task CreateTenantAsync(Command.CreateTenantCommand request)
    {
        var systemUser = UserData.GetSystemUser(ApiTestContextBase.DefaultTenantId);
        await apiContext.PostAsync($"{BaseUrl}", request, systemUser);
    }

    internal async Task<TenantDetailsResponse> GetTenantAsync(string tenantId)
    {
        var systemUser = UserData.GetSystemUser(ApiTestContextBase.DefaultTenantId);
        var result = await apiContext.GetAsync<TenantDetailsResponse>($"{BaseUrl}/{tenantId}", systemUser);

        return result.Value;
    }
}