using Newtonsoft.Json;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

/// <summary>
/// Need to use service discovery to get the tenant features
/// </summary>
public class TenantFeaturesHttpClient
{
    private const string tenantFeaturesEndpoint = "api/v1/features";
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly HttpClient _httpClient;

    public TenantFeaturesHttpClient(IUserDetailsProvider userDetailsProvider, HttpClient httpClient)
    {
        _userDetailsProvider = userDetailsProvider;
        _httpClient = httpClient;
    }

    public async Task<string[]> GetTenantFeaturesAsync(string tenantId)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return Array.Empty<string>();
        }

        var authenticatedClient = SystemInternalJwtTokenFactory.AddUserContext(_httpClient, UserData.GetSystemUser(tenantId));

        var response = await authenticatedClient.GetStringAsync(tenantFeaturesEndpoint);

        var featureIds = JsonConvert.DeserializeObject<string[]>(response);

        return featureIds ?? Array.Empty<string>();
    }
}