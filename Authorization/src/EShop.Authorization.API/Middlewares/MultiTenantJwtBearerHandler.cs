using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Constants;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EShop.Authorization.API.Middlewares;

public sealed class MultiTenantJwtBearerHandler : JwtBearerHandler
{
    private readonly IJwtTokenManager _jwtTokenManager;
    private readonly IUserTokenCachingService _tokenCachingService;
    private readonly ILogger<MultiTenantJwtBearerHandler> _logger;

    public MultiTenantJwtBearerHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtTokenManager jwtTokenManager,
        IUserTokenCachingService tokenCachingService) : base(options, logger, encoder)
    {
        _jwtTokenManager = jwtTokenManager;
        _tokenCachingService = tokenCachingService;
        _logger = logger.CreateLogger<MultiTenantJwtBearerHandler>();
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Extract token from Authorization header
        var tokenExtractionResult = ExtractTokenFromHeader();
        if (tokenExtractionResult.IsFailure)
        {
            return AuthenticateResult.Fail(tokenExtractionResult.Error.Message);
        }

        var accessToken = tokenExtractionResult.Value;

        // 2. Validate token WITH lifetime validation
        var principal = await _jwtTokenManager.ValidateActiveTokenAsync(accessToken, Context.RequestAborted);

        // 3. Validate token in cache (session management)
        var cacheValidationResult = await ValidateTokenInCacheAsync(principal, accessToken);
        if (cacheValidationResult.IsFailure)
        {
            _logger.LogWarning("Token validation failed: {Error}", cacheValidationResult.Error.Message);
            return AuthenticateResult.Fail(cacheValidationResult.Error.Message);
        }

        // 4. Create authentication ticket
        var ticket = CreateAuthenticationTicket(principal);

        _logger.LogInformation("Successfully authenticated user {UserId} for tenant {TenantId}",
            GetClaimValue(principal, ClaimTypes.NameIdentifier),
            GetClaimValue(principal, "tenant_id"));

        return AuthenticateResult.Success(ticket);
    }

    private Result<string> ExtractTokenFromHeader()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Result.Failure<string>(ErrorContants.Authentication.InvalidToken);
        }

        var token = Request.Headers.Authorization.ToString();
        var santitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(token);

        if (string.IsNullOrEmpty(santitizedToken))
        {
            return Result.Failure<string>(ErrorContants.Authentication.InvalidToken);
        }

        return Result.Success(santitizedToken);
    }


    private async Task<Result> ValidateTokenInCacheAsync(ClaimsPrincipal principal, string accessToken)
    {
        var userId = GetClaimValue(principal, ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        var cachedToken = await _tokenCachingService.TryGetTokenAsync(userId, Context.RequestAborted);

        if (cachedToken?.AccessToken != accessToken)
        {
            _logger.LogWarning("Token not found in cache or mismatch for user {UserId}", userId);
            return Result.Failure(ErrorContants.Authentication.TokenInvalidCache);
        }

        if (cachedToken.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Cached token expired for user {UserId}", userId);
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        return Result.Success();
    }

    private AuthenticationTicket CreateAuthenticationTicket(ClaimsPrincipal principal)
    {
        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(Options.TokenValidationParameters.ClockSkew.TotalMinutes)
        };

        return new AuthenticationTicket(principal, properties, Scheme.Name);
    }

    private static string? GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal.FindFirst(claimType)?.Value;
    }
}
