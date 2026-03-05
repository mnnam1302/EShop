using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.Managers.JwtTokens;
using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EShop.Shared.Authentication.Middlewares;

internal sealed class MultiTenantJwtBearerHandler : JwtBearerHandler
{
    private readonly IJwtTokenManager tokenManager;
    private readonly IUserTokenCachingService userTokenCaching;

    public MultiTenantJwtBearerHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtTokenManager tokenManager,
        IUserTokenCachingService userTokenCaching) : base(options, logger, encoder)
    {
        this.tokenManager = tokenManager;
        this.userTokenCaching = userTokenCaching;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var tokenExtractionResult = GetAccessTokenMetadata();
        if (tokenExtractionResult.IsFailure)
        {
            return AuthenticateResult.Fail(tokenExtractionResult.Error.Message);
        }

        var accessToken = tokenExtractionResult.Value;

        try
        {
            var principal = await tokenManager.GetPrincipalFromTokenAsync(accessToken, Context.RequestAborted);

            // Skip cache validation for internal S2S tokens (audience = "internal")
            var audience = GetClaimValue(principal, "aud");
            if (audience != "internal")
            {
                var cacheValidationResult = await ValidateTokenInCacheAsync(principal, accessToken);
                if (cacheValidationResult.IsFailure)
                {
                    return AuthenticateResult.Fail(cacheValidationResult.Error.Message);
                }
            }

            var ticket = CreateAuthenticationTicket(principal);
            return AuthenticateResult.Success(ticket);
        }
        catch (Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException ex)
        {
            Logger.LogWarning(ex, "JWT token expired during authentication");
            return AuthenticateResult.Fail("The provided token has expired.");
        }
        catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
        {
            Logger.LogWarning(ex, "JWT token validation failed during authentication");
            return AuthenticateResult.Fail("The provided token is invalid.");
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "Invalid operation during JWT token validation");
            return AuthenticateResult.Fail("Authentication failed due to configuration error.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unexpected exception during JWT authentication");
            return AuthenticateResult.Fail("Authentication failed.");
        }
    }

    private Result<string> GetAccessTokenMetadata()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Result.Failure<string>(new("Authentication.MissingAuthorization", "The Authorization header is missing."));
        }

        var token = Request.Headers.Authorization.ToString();
        var santitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(token);

        if (string.IsNullOrEmpty(santitizedToken))
        {
            return Result.Failure<string>(new("Authentication.InvalidToken", "The provided token must not null or empty."));
        }

        return Result.Success(santitizedToken);
    }

    private async Task<Result> ValidateTokenInCacheAsync(ClaimsPrincipal principal, string accessToken)
    {
        var userId = GetClaimValue(principal, EShopClaimTypes.UserId)
            ?? GetClaimValue(principal, ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure<string>(new("Authentication.InvalidToken", "The provided token must not null or empty."));
        }

        var cachedToken = await userTokenCaching.GetAsync(userId, Context.RequestAborted);

        // Degraded mode: If cache is unavailable (returns null), skip cache validation
        // and rely on JWT signature validation alone (which already passed if we got here)
        if (cachedToken is null)
        {
            Logger.LogWarning("Operating in degraded authentication mode: cache unavailable for user '{UserId}', skipping cache validation", userId);
            return Result.Success();
        }

        if (cachedToken.AccessToken != accessToken)
        {
            return Result.Failure(new("Authentication.InvalidToken", "The provided token does not match the cached token."));
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
