using EShop.Identity.Application.Abstractions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.DomainTools.DomainExceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using System.Security.Claims;

namespace EShop.Identity.Application.UseCases.V1.Queries.Users;

public class RefreshHandler : IQueryHandler<Query.Refresh, Response.AuthenticatedResponse>
{
    private readonly ITokenCachingService _tokenCacheService;
    private readonly ITokenService _tokenService;

    public RefreshHandler(ITokenCachingService cacheService, ITokenService tokenService)
    {
        _tokenCacheService = cacheService;
        _tokenService = tokenService;
    }

    public async Task<Result<Response.AuthenticatedResponse>> Handle(Query.Refresh request, CancellationToken cancellationToken)
    {
        // Verify access token and get userId
        var principles = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principles.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            throw new AuthorizationException("Invalid token");
        }

        // Check token verfied with info token caching
        var authenticatedCaching = IsValidToGenerateNewToken(userId, request);

        var accessToken = _tokenService.GenerateAccessToken(principles.Claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var newAuthenticatedCaching = new Response.AuthenticatedResponse
        {
            UserId = userId,
            UserName = authenticatedCaching.UserName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = authenticatedCaching.RefreshTokenExpiryTime
        };

        _tokenCacheService.AddToken(userId, newAuthenticatedCaching);
        return Result.Success(newAuthenticatedCaching);
    }

    private Response.AuthenticatedResponse IsValidToGenerateNewToken(string userId, Query.Refresh request)
    {
        _tokenCacheService.TryGetToken(userId, out var authenticatedCaching);

        if (authenticatedCaching == null ||
            authenticatedCaching.AccessToken != request.AccessToken)
        {
            throw new AuthorizationException("Invalid token");
        }

        if (authenticatedCaching?.RefreshToken != request.RefreshToken ||
            authenticatedCaching.RefreshTokenExpiryTime < DateTime.Now)
        {
            throw new AuthorizationException("Invalid token");
        }

        return authenticatedCaching;
    }
}