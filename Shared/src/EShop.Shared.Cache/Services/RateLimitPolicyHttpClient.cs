using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using System.Net.Http.Json;

namespace EShop.Shared.Cache.Services;

public sealed class RateLimitPolicyHttpClient(
    HttpClient httpClient,
    ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory)
{
    public async Task<CachedRateLimitPolicy?> GetRateLimitPolicyAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var authenticatedClient = await systemInternalJwtTokenFactory.AddUserContext(httpClient, UserData.GetSystemUser(tenantId), cancellationToken);
        var url = $"api/v1/tenants/{tenantId}/rate-limit-policy";

        var response = await authenticatedClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadFromJsonAsync<Result<CachedRateLimitPolicy>>(cancellationToken: cancellationToken);

        return content is { IsSuccess: true } ? content.Value : null;
    }
}
