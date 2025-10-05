using System.Security.Claims;

namespace EShop.Authorization.Application.Abstractions;

public interface IJwtTokenManager
{
    Task<string> GenerateAccessTokenAsync(IEnumerable<Claim> claims, string tenantId);
    string GenerateRefreshToken();
}
