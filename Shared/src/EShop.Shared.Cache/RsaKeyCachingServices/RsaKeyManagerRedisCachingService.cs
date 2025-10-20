using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace EShop.Shared.Cache.KeyEncryption;

public sealed class RsaKeyManagerRedisCachingService : IKeyManagerCachingService
{
    private readonly ILogger<RsaKeyManagerRedisCachingService> _logger;
    private readonly IRedisCachingProvider<RsaKeyPair> _rsaKeyPairCaching;
    private readonly IRedisCachingProvider<RsaPublicKeyCacheEntry> _publicKeyCaching;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public RsaKeyManagerRedisCachingService(
        ILogger<RsaKeyManagerRedisCachingService> logger,
        IRedisCachingProvider<RsaKeyPair> redisCachingProvider,
        IRedisCachingProvider<RsaPublicKeyCacheEntry> publicKeyCachingProvider,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _logger = logger;
        _rsaKeyPairCaching = redisCachingProvider;
        _publicKeyCaching = publicKeyCachingProvider;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task<RsaKeyPair?> TryGetActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = RsaCacheKeyProvider.GetRsaKeyPairCacheKey(tenantId);
        var keyPair = await _rsaKeyPairCaching.GetAsync(cacheKey);

        if (keyPair != null)
        {
            _logger.LogInformation("Retrieved active RSA key pair for tenant {TenantId} with KeyId {KeyId}", tenantId, keyPair.KeyId);
        }

        return keyPair;
    }

    public async Task AddActiveKeyPairAsync(string tenantId, RsaKeyPair keyPair)
    {
        var cacheKey = RsaCacheKeyProvider.GetRsaKeyPairCacheKey(tenantId);
        var options = CreateCacheOptions(keyPair.ExpiresAt);

        await _rsaKeyPairCaching.AddAsync(cacheKey, keyPair, options);

        _logger.LogInformation(
            "Cached active RSA key pair for tenant {TenantId} with KeyId {KeyId}, expires at {ExpiresAt}",
            tenantId, keyPair.KeyId, keyPair.ExpiresAt);
    }

    public async Task RemoveActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = RsaCacheKeyProvider.GetRsaKeyPairCacheKey(tenantId);
        await _rsaKeyPairCaching.ClearAsync(cacheKey);
        _logger.LogInformation("Removed active RSA key pair cache for tenant {TenantId}", tenantId);
    }

    public async Task<RSA?> TryGetPublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = RsaCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        var cachedEntry = await _publicKeyCaching.GetAsync(cacheKey);

        if (cachedEntry?.PublicKeyPem == null)
        {
            _logger.LogDebug("Public key not found in cache for tenant {TenantId} and keyId {KeyId}",
                tenantId, keyId);
            return null;
        }

        var rsa = RSA.Create();
        rsa.ImportFromPem(cachedEntry.PublicKeyPem);

        _logger.LogDebug("Retrieved and recreated RSA public key for tenant {TenantId} and keyId {KeyId}",
            tenantId, keyId);

        return rsa;
    }

    public async Task AddPublicKeyAsync(string tenantId, string keyId, RSA publicKey, DateTimeOffset expiresAt)
    {
        var cacheKey = RsaCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        var publicKeyPem = publicKey.ExportRSAPublicKeyPem();

        var cacheEntry = new RsaPublicKeyCacheEntry
        {
            KeyId = keyId,
            TenantId = tenantId,
            PublicKeyPem = publicKeyPem,
            ExpiresAt = expiresAt
        };

        var options = CreateCacheOptions(expiresAt);

        await _publicKeyCaching.AddAsync(cacheKey, cacheEntry, options);

        _logger.LogDebug("Cached RSA public key for tenant {TenantId} with keyId {KeyId}, expires at {ExpiresAt}",
            tenantId, keyId, expiresAt);
    }

    public async Task RemovePublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = RsaCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        await _publicKeyCaching.ClearAsync(cacheKey);

        _logger.LogDebug("Removed RSA public key cache for tenant {TenantId} and keyId {KeyId}",
            tenantId, keyId);
    }

    private DistributedCacheEntryOptions CreateCacheOptions(DateTimeOffset expiresAt)
    {
        return new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiresAt,
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingExpiration()
        };
    }
}
