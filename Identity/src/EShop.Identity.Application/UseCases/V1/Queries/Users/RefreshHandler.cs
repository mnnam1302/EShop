using EShop.Identity.Application.Abstractions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using System.Security.Claims;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

public class RefreshHandler : IQueryHandler<Query.Refresh, Response.AuthenticatedResponse>
{
    private readonly IUserTokenCachingService _tokenCacheService;
    private readonly ITokenService _tokenService;

    public RefreshHandler(IUserTokenCachingService cacheService, ITokenService tokenService)
    {
        _tokenCacheService = cacheService;
        _tokenService = tokenService;
    }

    public async Task<Result<Response.AuthenticatedResponse>> Handle(Query.Refresh request, CancellationToken cancellationToken)
    {
        var principles = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principles.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            throw new UnauthorizedException("Invalid token");
        }

        var authenticatedCaching = await ValidateAndRetrieveTokenAsync(userId, request);

        var accessToken = _tokenService.GenerateAccessToken(principles.Claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var newToken = new Response.AuthenticatedResponse
        {
            UserId = userId,
            UserName = authenticatedCaching.UserName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = authenticatedCaching.RefreshTokenExpiryTime.DateTime
        };

        // Convert to AuthenticationCaching for caching
        var authenticationCaching = new TokenAuthenticationCaching
        {
            UserId = newToken.UserId,
            UserName = newToken.UserName,
            AccessToken = newToken.AccessToken,
            RefreshToken = newToken.RefreshToken,
            RefreshTokenExpiryTime = new DateTimeOffset(newToken.RefreshTokenExpiryTime)
        };

        await _tokenCacheService.AddTokenAsync(userId, authenticationCaching);
        return Result.Success(newToken);
    }

    private async Task<TokenAuthenticationCaching> ValidateAndRetrieveTokenAsync(string userId, Query.Refresh request)
    {
        var tokenCached = await _tokenCacheService.TryGetTokenAsync(userId);

        if (tokenCached is null || tokenCached.AccessToken != request.AccessToken)
        {
            throw new UnauthorizedException("Invalid token");
        }

        if (tokenCached?.RefreshToken != request.RefreshToken || tokenCached.RefreshTokenExpiryTime < DateTimeOffset.Now)
        {
            throw new UnauthorizedException("Invalid token");
        }

        return tokenCached;
    }
}