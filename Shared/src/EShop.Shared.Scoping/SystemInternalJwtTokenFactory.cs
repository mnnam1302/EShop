using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace EShop.Shared.Scoping;

public static class SystemInternalJwtTokenFactory
{
    public static string Issuer { get; } = "SYSTEM-INTERNAL";
    public static string Audience { get; } = "SYSTEM-INTERNAL";
    public static SecurityKey SecurityKey { get; }
    public static SigningCredentials SigningCredentials { get; }

    private static readonly JsonWebTokenHandler _tokenHandler = new JsonWebTokenHandler();
    private static readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();
    private static readonly byte[] _securityKey = new byte[32];

    static SystemInternalJwtTokenFactory()
    {
        _randomNumberGenerator.GetBytes(_securityKey);
        SecurityKey = new SymmetricSecurityKey(_securityKey) { KeyId = Guid.NewGuid().ToString() };
        SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    }

    public static HttpClient AddUserContext(HttpClient client, UserData user)
    {
        var authenticationHeaderValue = GenerateAuthorizationHeaderValue(user);
        client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
        if (user.UserType is UserTypes.AppClientWithIndividualUsers or UserTypes.AppClientWithoutIndividualUsers)
        {
            AddDefaultCustomHeadersForUser(client, user);
        }
        return client;
    }
    public static AuthenticationHeaderValue GenerateAuthorizationHeaderValue(UserData user)
    {
        var tenantGroups = new List<string> { user.TenantId };
        if (user.IsSupportUser && user.TenantId != UserData.EShopSupportGroup)
        {
            tenantGroups.Add(UserData.EShopSupportGroup);
        }

        var tokenForSpecificUser = GenerateToken(user.Id, tenantGroups, user.Username);
        return new AuthenticationHeaderValue("Bearer", tokenForSpecificUser);
    }

    public static string GenerateToken(string userId, List<string> tenantGroups, string username, IDictionary<string, object>? additionalClaims = null, int expireInDays = 1)
    {
        var claims = new Dictionary<string, object>
        {
            ["jti"] = Guid.NewGuid().ToString(),
            ["sub"] = userId,
            ["username"] = username,
            ["tenant:groups"] = tenantGroups
        };

        if (additionalClaims != null)
        {
            foreach (var additionalClaim in additionalClaims)
            {
                claims.Add(additionalClaim.Key, additionalClaim.Value.ToString() ?? string.Empty);
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Claims = claims,
            Expires = DateTime.UtcNow.AddDays(expireInDays),
            SigningCredentials = SigningCredentials
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
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