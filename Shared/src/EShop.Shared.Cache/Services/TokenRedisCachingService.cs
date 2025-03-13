using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public class TokenRedisCachingService : ITokenCachingService
{
    private readonly IRedisCachingAsyncProvider<Response.AuthenticatedResponse> _redisCachingService;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public TokenRedisCachingService(
        IRedisCachingAsyncProvider<Response.AuthenticatedResponse> redisCachingService,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingService = redisCachingService;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task<Response.AuthenticatedResponse?> TryGetTokenAsync(string userId)
    {
        var cachedToken = await _redisCachingService.GetAsync(UserTokenCacheKeyProvider.GetCacheKey(userId));

        if (cachedToken is null)
        {
            throw new BadRequestException($"Invalid cached token for user '{userId}'");
        }

        return cachedToken;
    }

    public async Task AddTokenAsync(string userId, Response.AuthenticatedResponse token)
    {
        await _redisCachingService.AddAsync(
            UserTokenCacheKeyProvider.GetCacheKey(userId),
            token,
            new DistributedCacheEntryOptions { SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration() });
    }

    public async Task RemoveCacheAsync(string userId)
    {
        await _redisCachingService.ClearAsync(UserTokenCacheKeyProvider.GetCacheKey(userId));
    }
}