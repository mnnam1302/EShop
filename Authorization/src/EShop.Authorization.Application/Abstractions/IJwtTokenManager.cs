using System.Security.Claims;

namespace EShop.Authorization.Application.Abstractions;

public interface IJwtTokenManager
{
    Task<string> GenerateAccessTokenAsync(IEnumerable<Claim> claims, string tenantId, CancellationToken cancellationToken);
    string GenerateRefreshToken();
    Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token, CancellationToken cancellationToken);
}
