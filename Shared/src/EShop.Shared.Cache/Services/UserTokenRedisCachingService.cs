using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public sealed class UserTokenRedisCachingService : IUserTokenCachingService
{
    private readonly IRedisCachingProvider<TokenAuthenticationCaching> _redisCachingService;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public UserTokenRedisCachingService(
        IRedisCachingProvider<TokenAuthenticationCaching> redisCachingService,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingService = redisCachingService;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task<TokenAuthenticationCaching?> TryGetTokenAsync(string userId)
    {
        var cacheValue = await _redisCachingService.GetAsync(UserTokenCacheKeyProvider.GetCacheKey(userId));

        if (cacheValue is null)
        {
            throw new BadRequestException($"Invalid cached token for user '{userId}'");
        }

        return cacheValue;
    }

    public async Task AddTokenAsync(string userId, TokenAuthenticationCaching token)
    {
        var cacheKey = UserTokenCacheKeyProvider.GetCacheKey(userId);
        await _redisCachingService.AddAsync(cacheKey, token, new DistributedCacheEntryOptions
        {
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration()
        });
    }

    public async Task RemoveCacheAsync(string userId)
    {
        await _redisCachingService.ClearAsync(UserTokenCacheKeyProvider.GetCacheKey(userId));
    }
}