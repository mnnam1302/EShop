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
        var keyPair = await _rsaKeyManager.GetActiveKeyPairAsync(tenantId);
        if (keyPair == null)
        {
            throw new InvalidOperationException($"No RSA key pair found for tenant {tenantId}");
        }

        var rsaSecurityKey = new RsaSecurityKey(keyPair.PrivateKey) { KeyId = keyPair.KeyId };
        var signingCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var allClaims = EnrichClaimsWithStandardValues(claims, keyPair.KeyId, tenantId);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(allClaims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpiryMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private static List<Claim> EnrichClaimsWithStandardValues(IEnumerable<Claim> claims, string keyId, string tenantId)
    {
        var allClaims = claims.ToList();

        // Add JWT standard claims
        allClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        allClaims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

        // Add custom claims for validation
        allClaims.Add(new Claim("key_id", keyId));
        allClaims.Add(new Claim("token_version", "1.0")); // For future token format versioning

        // Ensure tenant_id is present
        if (!allClaims.Any(c => c.Type == "tenant_id"))
        {
            allClaims.Add(new Claim("tenant_id", tenantId));
        }

        return allClaims;
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

        var (tenantId, keyId) = ExtractTokenMetadata(jsonToken);
        var publicKey = await GetPublicKeyForValidation(tenantId, keyId);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = false, // Allow expired tokens for logout validation
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new RsaSecurityKey(publicKey) { KeyId = keyId },
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCulture))
        {
            throw new SecurityTokenException("Invalid token algorithm");
        }

        return principal;
    }

    private static (string tenantId, string keyId) ExtractTokenMetadata(JwtSecurityToken jsonToken)
    {
        var tenantId = jsonToken.Claims.FirstOrDefault(x => x.Type == "tenant_id")?.Value;
        var keyId = jsonToken.Claims.FirstOrDefault(x => x.Type == "key_id")?.Value;

        if (string.IsNullOrEmpty(tenantId))
            throw new SecurityTokenException("Token does not contain tenant_id claim");

        if (string.IsNullOrEmpty(keyId))
            throw new SecurityTokenException("Token does not contain key_id claim");

        return (tenantId, keyId);
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
