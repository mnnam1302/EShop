using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.Managers.JwtTokens;
using EShop.Shared.Contracts.Abstractions.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EShop.Shared.Authentication.Middlewares
{
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
            var tokenExtractionResult = GetTokenMetadata();
            if (tokenExtractionResult.IsFailure)
            {
                return AuthenticateResult.Fail(tokenExtractionResult.Error.Message);
            }

            var accessToken = tokenExtractionResult.Value;
            var principal = await tokenManager.GetPrincipalFromTokenAsync(accessToken, Context.RequestAborted);

            var cacheValidationResult = await ValidateTokenInCacheAsync(principal, accessToken);
            if (cacheValidationResult.IsFailure)
            {
                return AuthenticateResult.Fail(cacheValidationResult.Error.Message);
            }

            var ticket = CreateAuthenticationTicket(principal);

            return AuthenticateResult.Success(ticket);
        }

        private Result<string> GetTokenMetadata()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Result.Failure<string>(new("Authentication.MissingToken", "The Authorization header is missing."));
            }

            var token = Request.Headers.Authorization.ToString();
            var santitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(token);

            if (string.IsNullOrEmpty(santitizedToken))
            {
                return Result.Failure<string>(new("Authentication.InvalidToken", "The provided token is invalid."));
            }

            return Result.Success(santitizedToken);
        }

        private async Task<Result> ValidateTokenInCacheAsync(ClaimsPrincipal principal, string accessToken)
        {
            var userId = GetClaimValue(principal, EShopClaimTypes.UserId);
            if (string.IsNullOrEmpty(userId))
            {
                return Result.Failure(new("Authentication.InvalidToken", "The provided token is invalid."));
            }

            var cachedToken = await userTokenCaching.GetAsync(userId, Context.RequestAborted);

            if (cachedToken?.AccessToken != accessToken)
            {
                return Result.Failure(new("Authentication.InvalidToken", "The provided token is invalid."));
            }

            if (cachedToken.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
            {
                return Result.Failure(new("Authentication.TokenExpired", "The provided token has expired."));
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
}
