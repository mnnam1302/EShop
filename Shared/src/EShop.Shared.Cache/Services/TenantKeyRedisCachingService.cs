using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

internal sealed class TenantKeyRedisCachingService : ITenantKeyCachingService
{
    private readonly IRedisCachingProvider<RsaKeyPair> _redisCachingProvider;

    public TenantKeyRedisCachingService(IRedisCachingProvider<RsaKeyPair> redisCachingProvider)
    {
        _redisCachingProvider = redisCachingProvider;
    }

    public async Task AddAsync(string tenantId, RsaKeyPair keyPair, CancellationToken cancellationToken)
    {
        // Use active key cache key by default for backward compatibility
        await SetActiveKeyAsync(tenantId, keyPair, cancellationToken);
    }

    public async Task<RsaKeyPair?> GetAsync(string tenantId, CancellationToken cancellationToken)
    {
        // Try new active key first, then fall back to legacy key for migration
        var activeKey = await GetActiveKeyAsync(tenantId, cancellationToken);
        if (activeKey != null)
        {
            return activeKey;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        var cacheKey = RsaCacheKeyProvider.GetRsaKeyPairCacheKey(tenantId);
#pragma warning restore CS0618 // Type or member is obsolete
        return await _redisCachingProvider.GetAsync(cacheKey, cancellationToken);
    }

    public Task RemoveAsync(string tenantId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<RsaKeyPair?> GetActiveKeyAsync(string tenantId, CancellationToken cancellationToken)
    {
        var cacheKey = RsaCacheKeyProvider.GetActiveKeyCacheKey(tenantId);
        return await _redisCachingProvider.GetAsync(cacheKey, cancellationToken);
    }

    public async Task<RsaKeyPair?> GetPreviousKeyAsync(string tenantId, CancellationToken cancellationToken)
    {
        var cacheKey = RsaCacheKeyProvider.GetPreviousKeyCacheKey(tenantId);
        return await _redisCachingProvider.GetAsync(cacheKey, cancellationToken);
    }

    public async Task SetActiveKeyAsync(string tenantId, RsaKeyPair keyPair, CancellationToken cancellationToken)
    {
        var cacheKey = RsaCacheKeyProvider.GetActiveKeyCacheKey(tenantId);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = keyPair.ExpiresAt
        };

        await _redisCachingProvider.AddAsync(cacheKey, keyPair, options, cancellationToken);
    }

    public async Task SetPreviousKeyAsync(string tenantId, RsaKeyPair keyPair, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var cacheKey = RsaCacheKeyProvider.GetPreviousKeyCacheKey(tenantId);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await _redisCachingProvider.AddAsync(cacheKey, keyPair, options, cancellationToken);
    }
}
