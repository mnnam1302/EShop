using EShop.Authorization.Domain.Constants;
using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.Managers.JwtTokens;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using System.Security.Claims;

namespace EShop.Authorization.Application.UseCases.Authentication;

public sealed record RefreshTokenQuery(string AccessToken, string RefreshToken) : IQuery<AuthenticationResponse>;

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

        var tokenClaimsResult = await ParseAccessTokenAsync(query.AccessToken, cancellationToken);
        if (tokenClaimsResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(tokenClaimsResult.Error);
        }

        var tokenClaims = tokenClaimsResult.Value;

        var cachedTokenResult = await ValidateCachedTokensAsync(tokenClaims.UserId, query, cancellationToken);
        if (cachedTokenResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(cachedTokenResult.Error);
        }

        var cachedToken = cachedTokenResult.Value;
        var newTokens = await GenerateNewTokensAsync(tokenClaims, cachedToken.RefreshTokenExpiryTime, cancellationToken: cancellationToken);

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

    private async Task<Result<TokenClaims>> ParseAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        var sanitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(accessToken);
        var principal = await _jwtTokenManager.GetPrincipalFromExpiredToken(sanitizedToken, cancellationToken);

        var tokenClaims = GetTokenMetadata(principal);
        if (!tokenClaims.IsValid)
        {
            return Result.Failure<TokenClaims>(ErrorContants.Authentication.InvalidToken);
        }

        return Result.Success(tokenClaims);
    }

    private static TokenClaims GetTokenMetadata(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(EShopClaimTypes.UserId)?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantId = principal.FindFirst(EShopClaimTypes.TenantId)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
        {
            return new TokenClaims(string.Empty, string.Empty, Array.Empty<Claim>());
        }

        return new TokenClaims(userId, tenantId, principal.Claims);
    }

    private async Task<Result<TokenAuthentication>> ValidateCachedTokensAsync(string userId, RefreshTokenQuery query, CancellationToken cancellationToken)
    {
        var cachedToken = await _tokenCachingService.GetAsync(userId, cancellationToken);
        if (cachedToken is null)
        {
            return Result.Failure<TokenAuthentication>(ErrorContants.Authentication.TokenInvalidCache);
        }

        var tokenValidationResult = ValidateTokensMatch(cachedToken, query);
        if (tokenValidationResult.IsFailure)
        {
            return Result.Failure<TokenAuthentication>(tokenValidationResult.Error);
        }

        return Result.Success(cachedToken);
    }

    private static Result ValidateTokensMatch(TokenAuthentication cachedToken, RefreshTokenQuery query)
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

    private static bool AreTokensMatching(TokenAuthentication cachedToken, string accessToken, string refreshToken) =>
        cachedToken.AccessToken == accessToken && cachedToken.RefreshToken == refreshToken;

    private static bool IsRefreshTokenExpired(DateTimeOffset expiryTime) =>
        expiryTime <= DateTimeOffset.UtcNow;

    private async Task<AuthenticationResponse> GenerateNewTokensAsync(TokenClaims tokenClaims, DateTimeOffset refreshTokenExpiryTime, CancellationToken cancellationToken)
    {
        var newAccessToken = await _jwtTokenManager.GenerateAccessToken(tokenClaims.UserId, tokenClaims.TenantId, cancellationToken: cancellationToken);
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
        var tokenCache = new TokenAuthentication
        {
            UserId = response.UserId,
            UserName = response.UserId,
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            RefreshTokenExpiryTime = response.RefreshTokenExpiryTime
        };

        await _tokenCachingService.AddAsync(userId, tokenCache, cancellationToken);
    }

    private readonly record struct TokenClaims(string UserId, string TenantId, IEnumerable<Claim> Claims)
    {
        public readonly bool IsValid => !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(TenantId);
    }
}
