using EShop.Authorization.Application.Abstractions;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EShop.Authorization.Infrastructure.Authentication;

public sealed class JwtTokenManager : IJwtTokenManager
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenManager(IOptionsMonitor<JwtOptions> options)
    {
        _jwtOptions = options.CurrentValue;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        return string.Empty;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var refreshToken = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return refreshToken;
    }
}
