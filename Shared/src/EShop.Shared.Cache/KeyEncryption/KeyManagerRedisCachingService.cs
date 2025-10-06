using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;

namespace EShop.Shared.Cache.KeyEncryption;

public sealed class KeyManagerRedisCachingService : IKeyManagerCachingService
{
    private readonly IRedisCachingProvider<RsaKeyPair> _redisCachingProvider;
    private readonly IRedisCachingProvider<string> _publicKeyCachingProvider;

    public KeyManagerRedisCachingService(IRedisCachingProvider<RsaKeyPair> redisCachingProvider, IRedisCachingProvider<string> publicKeyCachingProvider)
    {
        _redisCachingProvider = redisCachingProvider;
        _publicKeyCachingProvider = publicKeyCachingProvider;
    }

    public async Task<RsaKeyPair?> TryGetActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        return await _redisCachingProvider.GetAsync(cacheKey);
    }

    public async Task AddActiveKeyPairAsync(string tenantId, RsaKeyPair keyPair)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        var options = CreateCacheOptions(keyPair.ExpiresAt);
        await _redisCachingProvider.AddAsync(cacheKey, keyPair, options);
    }

    public async Task RemoveActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        await _redisCachingProvider.ClearAsync(cacheKey);
    }

    public async Task<RSA?> TryGetPublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        var publicKeyPem = await _publicKeyCachingProvider.GetAsync(cacheKey);

        return string.IsNullOrEmpty(publicKeyPem) ? null : CreateRsaFromPem(publicKeyPem);
    }

    public async Task AddPublicKeyAsync(string tenantId, string keyId, RSA publicKey, DateTimeOffset expiresAt)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        var publicKeyPem = publicKey.ExportSubjectPublicKeyInfoPem();
        var options = CreateCacheOptions(expiresAt);

        await _publicKeyCachingProvider.AddAsync(cacheKey, publicKeyPem, options);
    }

    public async Task RemovePublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        await _publicKeyCachingProvider.ClearAsync(cacheKey);
    }

    private static DistributedCacheEntryOptions CreateCacheOptions(DateTimeOffset expiresAt)
    {
        return new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt };
    }

    private static RSA CreateRsaFromPem(string publicKeyPem)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        return rsa;
    }
}
