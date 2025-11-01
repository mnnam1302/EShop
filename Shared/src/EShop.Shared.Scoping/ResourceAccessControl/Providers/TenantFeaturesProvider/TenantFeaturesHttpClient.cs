using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using Newtonsoft.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

public sealed class TenantFeaturesHttpClient
{
    private const string tenantFeaturesEndpoint = "api/v1/features";

    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly HttpClient _httpClient;
    private readonly ISystemInternalJwtTokenFactory _systemInternalJwtTokenFactory;

    public TenantFeaturesHttpClient(
        IUserDetailsProvider userDetailsProvider,
        HttpClient httpClient,
        ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory)
    {
        _userDetailsProvider = userDetailsProvider;
        _httpClient = httpClient;
        _systemInternalJwtTokenFactory = systemInternalJwtTokenFactory;
    }

    public async Task<string[]> GetTenantFeaturesAsync(string tenantId)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return Array.Empty<string>();
        }

        var authenticatedClient = await _systemInternalJwtTokenFactory.AddUserContext(_httpClient, UserData.GetSystemUser(tenantId));

        var response = await authenticatedClient.GetStringAsync($"{tenantFeaturesEndpoint}");

        var features = JsonConvert.DeserializeObject<Result<Response.FeatureResponseInternal>>(response);
        return features?.Value.FeatureIds ?? Array.Empty<string>();
    }
}