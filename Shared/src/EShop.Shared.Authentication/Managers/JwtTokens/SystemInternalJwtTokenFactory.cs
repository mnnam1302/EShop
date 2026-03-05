using EShop.Shared.Authentication.Abstractions;
using System.Net.Http.Headers;

namespace EShop.Shared.Authentication.Managers.JwtTokens;

public sealed class SystemInternalJwtTokenFactory : ISystemInternalJwtTokenFactory
{
    private readonly IJwtTokenManager jwtTokenManager;

    public SystemInternalJwtTokenFactory(IJwtTokenManager jwtTokenManager)
    {
        this.jwtTokenManager = jwtTokenManager;
    }

    public async Task<HttpClient> AddUserContext(HttpClient client, UserData operationalUser, CancellationToken cancellationToken = default)
    {
        var accessToken = await GenerateAuthorizationHeaderValue(operationalUser);
        client.DefaultRequestHeaders.Authorization = accessToken;

        if (operationalUser.UserType is UserTypes.AppClientWithIndividualUsers or UserTypes.AppClientWithoutIndividualUsers)
        {
            AddDefaultCustomHeadersForUser(client, operationalUser);
        }

        return client;
    }

    private async Task<AuthenticationHeaderValue> GenerateAuthorizationHeaderValue(UserData user)
    {
        var tenantGroups = new List<string> { user.TenantId };
        if (user.IsSupportUser && user.TenantId != UserData.EShopSupportGroup)
        {
            tenantGroups.Add(UserData.EShopSupportGroup);
        }

        var tokenForSpecificUser = await GenerateToken(user, tenantGroups);
        return new AuthenticationHeaderValue("Bearer", tokenForSpecificUser);
    }

    private async Task<string> GenerateToken(UserData user, List<string> tenantGroups, IDictionary<string, object>? additionalClaims = null)
    {
        // Generate short-lived internal JWT with 30-second expiry
        // No cache write or refresh token for S2S calls
        var accessToken = await jwtTokenManager.GenerateAccessToken(
            user.Id,
            user.TenantId,
            additionalClaims,
            audienceOverride: "internal",
            expiryMinutes: 0.5, // 30 seconds
            CancellationToken.None);

        return accessToken;
    }

    private static void AddDefaultCustomHeadersForUser(HttpClient client, UserData user)
    {
        var headers = GetCustomHeadersForUser(user);

        foreach (var header in headers.Where(x => !client.DefaultRequestHeaders.Contains(x.Key)))
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }

    private static Dictionary<string, string> GetCustomHeadersForUser(UserData user)
    {
        return new Dictionary<string, string>
        {
            [HttpRequestUserDataProvider.UserTypeCustomHeaderName] = user.UserType,
            [HttpRequestUserDataProvider.UserIdCustomHeaderName] = user.Id,
            [HttpRequestUserDataProvider.TenantIdCustomHeaderName] = user.TenantId,
            [HttpRequestUserDataProvider.ActionUserIdCustomHeaderName] = user.ActionUserId
        };
    }
}