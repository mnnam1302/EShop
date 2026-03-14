using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.DependencyInjections;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EShop.Shared.Authentication.Managers.JwtTokens;

internal sealed class JwtTokenManager : IJwtTokenManager
{
    private readonly ITenantKeyProvider _rsaKeyManager;
    private readonly JwtOptions _jwtOptions;

    public JwtTokenManager(ITenantKeyProvider rsaKeyManager, IOptionsMonitor<JwtOptions> jwtOptions)
    {
        _rsaKeyManager = rsaKeyManager;
        _jwtOptions = jwtOptions.CurrentValue;
    }

    public async Task<string> GenerateAccessToken(
        string userId,
        string tenantId,
        IDictionary<string, object>? additionalClaims = null,
        string? audienceOverride = null,
        double? expiryMinutes = null,
        CancellationToken cancellationToken = default)
    {
        var keyPair = await _rsaKeyManager.GetOrCreateKeyPairAsync(tenantId, cancellationToken);

        var rsaSecurityKey = new RsaSecurityKey(keyPair.GetPrivateKey())
        {
            KeyId = keyPair.KeyId
        };

        var claims = SystemClaimFactory.Create(userId, tenantId, keyPair.KeyId, additionalClaims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes ?? _jwtOptions.AccessTokenExpiryMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = audienceOverride ?? _jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
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

    public async Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var tenantId = GetTenantIdFromClaims(jsonToken);
        var tokenKid = jsonToken.Header.Kid;

        // Try validating with active key first
        var activeKeyPair = await _rsaKeyManager.GetKeyPairAsync(tenantId, cancellationToken);

        try
        {
            return ValidateTokenWithKey(token, activeKeyPair, tokenHandler);
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            // Active key didn't work - try previous key if kid matches
            var previousKeyPair = await _rsaKeyManager.GetPreviousKeyPairAsync(tenantId, cancellationToken);

            if (previousKeyPair != null && previousKeyPair.KeyId == tokenKid)
            {
                return ValidateTokenWithKey(token, previousKeyPair, tokenHandler);
            }

            // No matching previous key or kid doesn't match
            throw;
        }
    }

    private ClaimsPrincipal ValidateTokenWithKey(string token, RsaKeyPair rsaKeyPair, JwtSecurityTokenHandler tokenHandler)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true, // CRITICAL: Validate token expiration
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudiences = [_jwtOptions.Audience, "internal"],
            IssuerSigningKey = new RsaSecurityKey(rsaKeyPair.GetPublicKey()) { KeyId = rsaKeyPair.KeyId },
            ClockSkew = TimeSpan.Zero,
        };

        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCulture))
        {
            throw new SecurityTokenException("Invalid token algorithm");
        }

        return principal;
    }

    public async Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var tenantId = GetTenantIdFromClaims(jsonToken);
        var tokenKid = jsonToken.Header.Kid;

        // Try validating with active key first
        var activeKeyPair = await _rsaKeyManager.GetKeyPairAsync(tenantId, cancellationToken);

        try
        {
            return ValidateExpiredTokenWithKey(token, activeKeyPair, tokenHandler);
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            // Active key didn't work - try previous key if kid matches
            var previousKeyPair = await _rsaKeyManager.GetPreviousKeyPairAsync(tenantId, cancellationToken);

            if (previousKeyPair != null && previousKeyPair.KeyId == tokenKid)
            {
                return ValidateExpiredTokenWithKey(token, previousKeyPair, tokenHandler);
            }

            // No matching previous key or kid doesn't match
            throw;
        }
    }

    private ClaimsPrincipal ValidateExpiredTokenWithKey(string token, RsaKeyPair rsaKeyPair, JwtSecurityTokenHandler tokenHandler)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudiences = [_jwtOptions.Audience, "internal"],
            IssuerSigningKey = new RsaSecurityKey(rsaKeyPair.GetPublicKey()) { KeyId = rsaKeyPair.KeyId },
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

    private static string GetTenantIdFromClaims(JwtSecurityToken jsonToken)
    {
        var tenantId = jsonToken.Claims.FirstOrDefault(x => x.Type == EShopClaimTypes.TenantId)?.Value;

        if (string.IsNullOrEmpty(tenantId))
        {
            throw new SecurityTokenException("Token does not contain tenant_id claim");
        }

        return tenantId;
    }
}