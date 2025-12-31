using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

// Extending HttpClient With Delegating Handlers in ASP.NET Core
public sealed class TenancyHttpClient(
    HttpClient httpClient,
    IUserDetailsProvider userDetailsProvider,
    ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory)
{
    public async Task<string[]> GetTenantFeaturesAsync(string tenantId)
    {
        try
        {
            if (!userDetailsProvider.IsAuthenticatedUser)
            {
                return [];
            }

            var authenticatedClient = await systemInternalJwtTokenFactory.AddUserContext(
                httpClient,
                UserData.GetSystemUser(tenantId));

            var response = await authenticatedClient.GetAsync("api/v1/features");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<Result<Response.FeatureResponseInternal>>();

            return content?.Value.FeatureIds ?? [];
        }
        catch (Exception ex)
        {
            return [];
        }
    }
}