using System.Security.Claims;

namespace EShop.Shared.Authentication.Abstractions
{
    public interface IJwtTokenManager
    {
        Task<string> GenerateAccessToken(string userId, string tenantId, IDictionary<string, object>? additionalClaims = null, CancellationToken cancellationToken = default);

        string GenerateRefreshToken();

        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token, CancellationToken cancellationToken = default);

        Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token, CancellationToken cancellationToken = default);
    }
}
