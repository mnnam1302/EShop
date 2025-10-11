using System.Security.Claims;

namespace EShop.Authorization.Application.Abstractions;

public interface IJwtTokenManager
{
    Task<string> GenerateAccessTokenAsync(IEnumerable<Claim> claims, string tenantId);
    string GenerateRefreshToken();

    /// <summary>
    /// Validates an active (non-expired) token for authentication purposes.
    /// Throws SecurityTokenExpiredException if token is expired.
    /// </summary>
    Task<ClaimsPrincipal> ValidateActiveTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>
    /// Validates token structure and signature but ignores expiration.
    /// Used for refresh token scenarios.
    /// </summary>
    Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token, CancellationToken cancellationToken);
}
