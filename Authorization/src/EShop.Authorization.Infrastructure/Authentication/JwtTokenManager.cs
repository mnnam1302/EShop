using EShop.Authorization.Application.Abstractions;
using EShop.Shared.Scoping.DependencyInjections.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EShop.Authorization.Infrastructure.Authentication;

public sealed class JwtTokenManager : IJwtTokenManager
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
        // 1. Get the active RSA key pair for the tenant
        var keyPair = await _rsaKeyManager.GetActiveKeyPairAsync(tenantId);
        if (keyPair == null)
        {
            throw new InvalidOperationException($"No RSA key pair found for tenant {tenantId}");
        }

        // 2. Create RSA security key
        var rsaSecurityKey = new RsaSecurityKey(keyPair.PrivateKey)
        {
            KeyId = keyPair.KeyId
        };

        // 3. Create signing credentials with RSA-SHA256
        var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        // 4. Add standard claims
        var allClaims = claims.ToList();
        allClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        allClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
        allClaims.Add(new Claim("key_id", keyPair.KeyId));

        // 5. Create JWT token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(allClaims),
            Expires = DateTime.UtcNow.AddHours(_jwtOptions.AccessTokenExpiryMinutes),
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

    public async Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token, CancellationToken cancellationToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var jsonToken = tokenHandler.ReadJwtToken(token);

        var tenantId = jsonToken.Claims.FirstOrDefault(x => x.Type == "tenant_id")?.Value;
        var keyId = jsonToken.Claims.FirstOrDefault(x => x.Type == "key_id")?.Value;

        if (string.IsNullOrEmpty(tenantId))
        {
            throw new SecurityTokenException("Token does not contain tenant_id claim");
        }

        if (string.IsNullOrEmpty(keyId))
        {
            throw new SecurityTokenException("Token does not contain key_id claim");
        }

        var publicKey = await GetPublicKeyForValidation(tenantId, keyId);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = false, // We explicitly want to allow expired tokens
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new RsaSecurityKey(publicKey) { KeyId = keyId },
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCulture))
        {
            throw new SecurityTokenException("Invalid token algorithm");
        }

        return principal;
    }

    private async Task<RSA> GetPublicKeyForValidation(string tenantId, string keyId)
    {
        try
        {
            var publicKey = await _rsaKeyManager.GetPublicKeyAsync(tenantId, keyId);
            return publicKey;
        }
        catch (KeyNotFoundException)
        {
            throw new SecurityTokenException($"Public key not found for tenant {tenantId} and keyId {keyId}");
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"Failed to retrieve public key for token validation: {ex.Message}", ex);
        }
    }
}
