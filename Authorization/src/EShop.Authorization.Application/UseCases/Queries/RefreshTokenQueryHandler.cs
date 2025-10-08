using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Constants;
using EShop.Authorization.Domain.Repositories;
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
    private readonly IUserRepository _userRepository;

    public RefreshTokenQueryHandler(IJwtTokenManager jwtTokenManager, IUserTokenCachingService tokenCachingService, IUserRepository userRepository)
    {
        _jwtTokenManager = jwtTokenManager;
        _tokenCachingService = tokenCachingService;
        _userRepository = userRepository;
    }

    public async Task<Result<AuthenticationResponse>> HandleAsync(RefreshTokenQuery query, CancellationToken cancellationToken = default)
    {
        // 1. Validate input
        if (string.IsNullOrWhiteSpace(query.AccessToken) || string.IsNullOrWhiteSpace(query.RefreshToken))
        {
            return Result.Failure<AuthenticationResponse>(ErrorContants.Authentication.InvalidToken);
        }

        // 2. Extract and validate token claims
        var claimsResult = await ExtractTokenClaimsAsync(query.AccessToken, cancellationToken);
        if (claimsResult.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(claimsResult.Error);
        }

        var (userId, tenantId, claims) = claimsResult.Value;

        // 3. Validate cached tokens
        var cacheValidation = await ValidateTokenCacheAsync(userId, query, cancellationToken);
        if (cacheValidation.IsFailure)
        {
            return Result.Failure<AuthenticationResponse>(cacheValidation.Error);
        }

        var cachedToken = cacheValidation.Value;

        // 4. Generate new tokens
        var newTokens = await GenerateNewTokensAsync(claims, tenantId, cachedToken.RefreshTokenExpiryTime);

        // 5. Update cache
        await UpdateTokenCacheAsync(userId, newTokens, cancellationToken);

        return Result.Success(newTokens);
    }

    private async Task<Result<(string UserId, string TenantId, IEnumerable<Claim> Claims)>> ExtractTokenClaimsAsync(
        string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var sanitizedToken = JwtEncodedStringHelper.GetJwtEncodedString(accessToken);
            var principal = await _jwtTokenManager.GetPrincipalFromExpiredToken(sanitizedToken, cancellationToken);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tenantId = principal.FindFirst("tenant_id")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            {
                return Result.Failure<(string, string, IEnumerable<Claim>)>(ErrorContants.Authentication.InvalidToken);
            }

            return Result.Success((userId, tenantId, principal.Claims));
        }
        catch
        {
            return Result.Failure<(string, string, IEnumerable<Claim>)>(ErrorContants.Authentication.InvalidToken);
        }
    }

    private async Task<Result<TokenAuthenticationCaching>> ValidateTokenCacheAsync(string userId, RefreshTokenQuery query, CancellationToken cancellationToken)
    {
        var tokenCached = await _tokenCachingService.TryGetTokenAsync(userId, cancellationToken);

        if (tokenCached is null)
        {
            return Result.Failure<TokenAuthenticationCaching>(ErrorContants.Authentication.TokenInvalidCache);
        }

        var sanitizedAccessToken = JwtEncodedStringHelper.GetJwtEncodedString(query.AccessToken);

        if (tokenCached.AccessToken != sanitizedAccessToken ||
            tokenCached.RefreshToken != query.RefreshToken ||
            tokenCached.RefreshTokenExpiryTime <= DateTimeOffset.UtcNow)
        {
            return Result.Failure<TokenAuthenticationCaching>(ErrorContants.Authentication.InvalidToken);
        }

        return Result.Success(tokenCached);
    }

    private async Task<AuthenticationResponse> GenerateNewTokensAsync(IEnumerable<Claim> claims, string tenantId, DateTimeOffset refreshTokenExpiryTime)
    {
        var accessToken = await _jwtTokenManager.GenerateAccessTokenAsync(claims, tenantId);
        var refreshToken = _jwtTokenManager.GenerateRefreshToken();

        return new AuthenticationResponse
        {
            UserId = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = refreshTokenExpiryTime
        };
    }

    private async Task UpdateTokenCacheAsync(string userId, AuthenticationResponse response, CancellationToken cancellationToken)
    {
        var authenticationCaching = new TokenAuthenticationCaching
        {
            UserId = response.UserId,
            UserName = response.UserId, // Using UserId as UserName to match pattern
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            RefreshTokenExpiryTime = response.RefreshTokenExpiryTime
        };

        await _tokenCachingService.AddTokenAsync(userId, authenticationCaching, cancellationToken);
    }
}
