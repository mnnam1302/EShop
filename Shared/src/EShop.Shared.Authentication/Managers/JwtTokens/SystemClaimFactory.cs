using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EShop.Shared.Authentication.Managers.JwtTokens;

public static class SystemClaimFactory
{
    public static IEnumerable<Claim> Create(string userId, string tenantId, string keyId, IDictionary<string, object>? additionalClaims)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(EShopClaimTypes.UserId, userId),
            new Claim(EShopClaimTypes.TenantId, tenantId),
            new Claim(EShopClaimTypes.KeyId, keyId),
        };

        if (additionalClaims != null)
        {
            foreach (var claim in additionalClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value.ToString() ?? string.Empty));
            }
        }

        return claims;
    }
}
