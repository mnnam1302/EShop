using System.Security.Claims;

namespace EShop.Authorization.Application.Abstractions;

public interface IJwtTokenManager
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) access token for the specified claims and tenant.
    /// </summary>
    /// <param name="claims">A collection of claims to include in the token. Each claim represents a piece of information about the user or
    /// entity.</param>
    /// <param name="tenantId">The unique identifier of the tenant for which the token is being generated. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated JWT access token as a
    /// string.</returns>
    Task<string> GenerateAccessTokenAsync(IEnumerable<Claim> claims, string tenantId);

    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
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
