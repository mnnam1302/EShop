using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Constants;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using System.Security.Claims;

namespace EShop.Authorization.Application.UseCases.Queries;

public sealed class RefreshTokenQuery : IQuery<AuthenticationResponse>
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}

internal sealed class RefreshTokenQueryHandler : IQueryHandler<RefreshTokenQuery, AuthenticationResponse>
{
    private readonly IJwtTokenManager _jwtTokenManager;
    private readonly IUserTokenCachingService _tokenCachingService;

    public RefreshTokenQueryHandler(IJwtTokenManager jwtTokenManager, IUserTokenCachingService tokenCachingService)
    {
        _jwtTokenManager = jwtTokenManager;
        _tokenCachingService = tokenCachingService;
    }

    public async Task<Result<AuthenticationResponse>> HandleAsync(RefreshTokenQuery query, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateTokenInputs(query);
        if (validationResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(validationResult.Error);
        }

        var tokenClaimsResult = await ParseAndValidateAccessTokenAsync(query.AccessToken, cancellationToken);
        if (tokenClaimsResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(tokenClaimsResult.Error);
        }

        var tokenClaims = tokenClaimsResult.Value;

        var cachedTokenResult = await ValidateCachedTokensAsync(tokenClaims.UserId!, query, cancellationToken);
        if (cachedTokenResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(cachedTokenResult.Error);
        }

        var cachedToken = cachedTokenResult.Value;
        var newTokens = await IssueNewTokensAsync(tokenClaims, cachedToken.RefreshTokenExpiryTime);

        await StoreCachedTokensAsync(tokenClaims.UserId, newTokens, cancellationToken);

        return Result.Success(newTokens);
    }

    private static Result ValidateTokenInputs(RefreshTokenQuery query)
    {
        if (string.IsNullOrEmpty(query.AccessToken) || string.IsNullOrEmpty(query.RefreshToken))
        {
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        return Result.Success();
    }

    private async Task<Result<TokenClaims>> ParseAndValidateAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var sanitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(accessToken);
            var principal = await _jwtTokenManager.GetPrincipalFromExpiredToken(sanitizedToken, cancellationToken);

            var tokenClaims = ExtractTokenClaims(principal);
            if (!tokenClaims.IsValid)
            {
                return Result.Failure<TokenClaims>(ErrorContants.Authentication.InvalidToken);
            }

            return Result.Success(tokenClaims);
        }
        catch (Exception)
        {
            return Result.Failure<TokenClaims>(ErrorContants.Authentication.InvalidToken);
        }
    }

    private static TokenClaims ExtractTokenClaims(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = principal.FindFirst("tenant_id")?.Value;

        return new TokenClaims(userId, tenantId, principal.Claims);
    }

    private async Task<Result<TokenAuthenticationCaching>> ValidateCachedTokensAsync(string userId, RefreshTokenQuery query, CancellationToken cancellationToken)
    {
        var cachedToken = await _tokenCachingService.TryGetTokenAsync(userId, cancellationToken);
        if (cachedToken is null)
        {
            return Result.Failure<TokenAuthenticationCaching>(ErrorContants.Authentication.TokenInvalidCache);
        }

        var tokenValidationResult = ValidateTokensMatch(cachedToken, query);
        if (tokenValidationResult.IsFailure)
        {
            return Result.Failure<TokenAuthenticationCaching>(tokenValidationResult.Error);
        }

        return Result.Success(cachedToken);
    }

    private static Result ValidateTokensMatch(TokenAuthenticationCaching cachedToken, RefreshTokenQuery query)
    {
        var sanitizedAccessToken = JwtEncodedStringHelper.GetJwtEncodedString(query.AccessToken);

        if (!AreTokensMatching(cachedToken, sanitizedAccessToken, query.RefreshToken))
        {
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        if (IsRefreshTokenExpired(cachedToken.RefreshTokenExpiryTime))
        {
            return Result.Failure(ErrorContants.Authentication.InvalidToken);
        }

        return Result.Success();
    }

    private static bool AreTokensMatching(TokenAuthenticationCaching cachedToken, string accessToken, string refreshToken) =>
        cachedToken.AccessToken == accessToken && cachedToken.RefreshToken == refreshToken;

    private static bool IsRefreshTokenExpired(DateTimeOffset expiryTime) =>
        expiryTime <= DateTimeOffset.UtcNow;

    private async Task<AuthenticationResponse> IssueNewTokensAsync(TokenClaims tokenClaims, DateTimeOffset refreshTokenExpiryTime)
    {
        var newAccessToken = await _jwtTokenManager.GenerateAccessTokenAsync(tokenClaims.Claims, tokenClaims.TenantId);
        var newRefreshToken = _jwtTokenManager.GenerateRefreshToken();

        return new AuthenticationResponse
        {
            UserId = tokenClaims.UserId!,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiryTime
        };
    }

    private async Task StoreCachedTokensAsync(string userId, AuthenticationResponse response, CancellationToken cancellationToken)
    {
        var tokenCache = new TokenAuthenticationCaching
        {
            UserId = response.UserId,
            UserName = response.UserId, // Following established pattern
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            RefreshTokenExpiryTime = response.RefreshTokenExpiryTime
        };

        await _tokenCachingService.AddTokenAsync(userId, tokenCache, cancellationToken);
    }

    private readonly record struct TokenClaims(string? UserId, string? TenantId, IEnumerable<Claim> Claims)
    {
        public readonly bool IsValid => !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(TenantId);
    }
}
