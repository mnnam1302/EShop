using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public sealed class UserTokenRedisCachingService : IUserTokenCachingService
{
    private readonly IRedisCachingProvider<TokenAuthentication> _redisCachingService;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public UserTokenRedisCachingService(
        IRedisCachingProvider<TokenAuthentication> redisCachingService,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingService = redisCachingService;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task<TokenAuthentication?> TryGetTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cachedToken = await _redisCachingService.GetAsync(UserTokenCacheKeyProvider.GetCacheKey(userId), cancellationToken);

        if (cachedToken is null)
        {
            throw new BadRequestException($"Invalid cached token for user '{userId}'");
        }

        return cachedToken;
    }

    public async Task AddTokenAsync(string userId, TokenAuthentication token, CancellationToken cancellationToken = default)
    {
        var cacheKey = UserTokenCacheKeyProvider.GetCacheKey(userId);
        await _redisCachingService.AddAsync(cacheKey, token, new DistributedCacheEntryOptions
        {
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration()
        }, cancellationToken);
    }

    public async Task RemoveCacheAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = UserTokenCacheKeyProvider.GetCacheKey(userId);
        await _redisCachingService.ClearAsync(cacheKey, cancellationToken);
    }
}