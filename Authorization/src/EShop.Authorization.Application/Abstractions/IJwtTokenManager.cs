using System.Security.Claims;

namespace EShop.Authorization.Application.Abstractions;

public interface IJwtTokenManager
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
}
