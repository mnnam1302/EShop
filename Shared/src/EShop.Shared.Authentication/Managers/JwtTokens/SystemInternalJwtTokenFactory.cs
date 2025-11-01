using EShop.Shared.Authentication.Abstractions;
using System.Net.Http.Headers;

namespace EShop.Shared.Authentication.Managers.JwtTokens;

public sealed class SystemInternalJwtTokenFactory : ISystemInternalJwtTokenFactory
{
    private readonly IJwtTokenManager jwtTokenManager;
    private readonly IUserTokenCachingService userTokenCachingService;

    public SystemInternalJwtTokenFactory(IJwtTokenManager jwtTokenManager, IUserTokenCachingService userTokenCachingService)
    {
        this.jwtTokenManager = jwtTokenManager;
        this.userTokenCachingService = userTokenCachingService;
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
        var accessToken = await jwtTokenManager.GenerateAccessToken(user.Id, user.TenantId, additionalClaims, CancellationToken.None);
        var refreshToken = jwtTokenManager.GenerateRefreshToken();

        var authenticationValue = new TokenAuthentication
        {
            UserId = user.Id,
            UserName = user.Username,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddDays(7)
        };

        await userTokenCachingService.AddAsync(user.Id, authenticationValue, CancellationToken.None);

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