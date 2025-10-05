using EShop.Authorization.Application.Abstractions;
using EShop.Shared.Scoping.DependencyInjections.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EShop.Authorization.Infrastructure.Authentication;

internal sealed class JwtTokenManager : IJwtTokenManager
{
    private readonly JwtOptions _jwtOptions;
    private readonly IRsaKeyManager _rsaKeyManager;

    public JwtTokenManager(IOptionsMonitor<JwtOptions> options, IRsaKeyManager rsaKeyManager)
    {
        _jwtOptions = options.CurrentValue;
        _rsaKeyManager = rsaKeyManager;
    }

    public async Task<string> GenerateAccessTokenAsync(IEnumerable<Claim> claims, string tenantId)
    {
        // Get the active RSA key pair for the tenant
        var keyPair = await _rsaKeyManager.GetActiveKeyPairAsync(tenantId);
        if (keyPair == null)
        {
            throw new InvalidOperationException($"No RSA key pair found for tenant {tenantId}");
        }

        // Create RSA security key
        var rsaSecurityKey = new RsaSecurityKey(keyPair.PrivateKey)
        {
            KeyId = keyPair.KeyId
        };

        // Create signing credentials with RSA-SHA256
        var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        // Add standard claims
        var allClaims = claims.ToList();
        allClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        allClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
        allClaims.Add(new Claim("key_id", keyPair.KeyId));

        // Create JWT token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(allClaims),
            Expires = DateTime.UtcNow.AddHours(_jwtOptions.AccessTokenExpiryHours),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
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
