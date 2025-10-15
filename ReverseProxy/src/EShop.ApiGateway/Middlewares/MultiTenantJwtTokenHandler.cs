using EShop.Shared.Cache.KeyEncryption;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Scoping.DependencyInjections;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EShop.ApiGateway.Middlewares;

public sealed class MultiTenantJwtBearerHandler(
    IOptionsMonitor<JwtBearerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUserTokenCachingService tokenCachingService,
    IKeyManagerCachingService keyManagerCaching,
    IOptionsMonitor<JwtOptions> jwtOptions) : JwtBearerHandler(options, logger, encoder)
{
    private readonly ILogger _logger = logger.CreateLogger<MultiTenantJwtBearerHandler>();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var tokenExtractionResult = ExtractTokenFromHeader();
        if (tokenExtractionResult.IsFailure)
        {
            _logger.LogWarning("Token extraction failed: {Error}", tokenExtractionResult.Error.Message);
            return AuthenticateResult.Fail(tokenExtractionResult.Error.Message);
        }

        var accessToken = tokenExtractionResult.Value;
        var principal = await ValidateTokenAsync(accessToken);

        var cacheValidationResult = await ValidateTokenInCacheAsync(principal, accessToken);
        if (cacheValidationResult.IsFailure)
        {
            _logger.LogWarning("Cache validation failed: {Error}", cacheValidationResult.Error.Message);
            return AuthenticateResult.Fail(cacheValidationResult.Error.Message);
        }

        var ticket = CreateAuthenticationTicket(principal);
        return AuthenticateResult.Success(ticket);
    }

    private Result<string> ExtractTokenFromHeader()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Result.Failure<string>(new("Authentication.InvalidToken", "Authorization header is missing"));
        }

        var token = Request.Headers.Authorization.ToString();
        var sanitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(token);

        if (string.IsNullOrEmpty(sanitizedToken))
        {
            return Result.Failure<string>(new("Authentication.InvalidToken", "The provided token is invalid or malformed"));
        }

        return Result.Success(sanitizedToken);
    }

    private async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        var (tenantId, keyId) = ExtractTokenMetadata(jsonToken);
        var publicKey = await keyManagerCaching.TryGetPublicKeyAsync(tenantId, keyId);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.CurrentValue.Issuer,
            ValidAudience = jwtOptions.CurrentValue.Audience,
            IssuerSigningKey = new RsaSecurityKey(publicKey) { KeyId = keyId },
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

    private static (string tenantId, string keyId) ExtractTokenMetadata(JwtSecurityToken jsonToken)
    {
        var tenantId = jsonToken.Claims.FirstOrDefault(x => x.Type == EShopClaimTypes.TenantId)?.Value;
        var keyId = jsonToken.Claims.FirstOrDefault(x => x.Type == EShopClaimTypes.KeyId)?.Value;

        if (string.IsNullOrEmpty(tenantId))
            throw new SecurityTokenException("Token does not contain tenant_id claim");

        if (string.IsNullOrEmpty(keyId))
            throw new SecurityTokenException("Token does not contain key_id claim");

        return (tenantId, keyId);
    }

    private async Task<Result> ValidateTokenInCacheAsync(ClaimsPrincipal principal, string accessToken)
    {
        var userId = GetClaimValue(principal, EShopClaimTypes.UserId);
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure(new("Authentication.InvalidToken", "User ID claim not found"));
        }

        var cachedToken = await tokenCachingService.TryGetTokenAsync(userId, Context.RequestAborted);

        if (cachedToken?.AccessToken != accessToken)
        {
            _logger.LogWarning("Token mismatch for user {UserId}", userId);
            return Result.Failure(new("Authentication.TokenInvalidCache", "Token not found in cache or has been revoked"));
        }

        if (cachedToken.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user {UserId}", userId);
            return Result.Failure(new("Authentication.InvalidToken", "Token session has expired"));
        }

        return Result.Success();
    }

    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }

    private AuthenticationTicket CreateAuthenticationTicket(ClaimsPrincipal principal)
    {
        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.CurrentValue.AccessTokenExpiryMinutes)
        };

        return new AuthenticationTicket(principal, properties, Scheme.Name);
    }
}
