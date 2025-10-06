using EShop.Shared.Cache.CacheKeys;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text.Json;

namespace EShop.Shared.Cache.KeyEncryption;

public sealed class KeyManagerRedisCachingService : IKeyManagerCachingService
{
    private readonly IDistributedCache _distributedCache;

    public KeyManagerRedisCachingService(IDistributedCache cache)
    {
        _distributedCache = cache;
    }

    public async Task<RsaKeyPair?> TryGetActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        var serializedKeyPair = await _distributedCache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(serializedKeyPair))
        {
            return null;
        }

        return DeserializeKeyPair(serializedKeyPair);
    }

    public async Task AddActiveKeyPairAsync(string tenantId, RsaKeyPair keyPair)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        var serializedKeyPair = SerializeKeyPair(keyPair);
        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = keyPair.ExpiresAt };
        await _distributedCache.SetStringAsync(cacheKey, serializedKeyPair, options);
    }

    public async Task RemoveActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        await _distributedCache.RemoveAsync(cacheKey);
    }

    public async Task<RSA?> TryGetPublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        var publicKeyPem = await _distributedCache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(publicKeyPem))
        {
            return null;
        }

        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        return rsa;
    }

    public async Task AddPublicKeyAsync(string tenantId, string keyId, RSA publicKey, DateTimeOffset expiresAt)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        var publicKeyPem = publicKey.ExportSubjectPublicKeyInfoPem();
        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt };
        await _distributedCache.SetStringAsync(cacheKey, publicKeyPem, options);
    }

    public async Task RemovePublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        await _distributedCache.RemoveAsync(cacheKey);
    }

    // Keep the legacy methods for backward compatibility with RsaKeyManager
    public async Task SetActiveKeyPairAsync(string tenantId, string serializedKeyPair, DateTimeOffset expiresAt)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt };
        await _distributedCache.SetStringAsync(cacheKey, serializedKeyPair, options);
    }

    public async Task<string?> GetActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetActiveKeyPairCacheKey(tenantId);
        return await _distributedCache.GetStringAsync(cacheKey);
    }

    public async Task SetPublicKeyAsync(string tenantId, string keyId, string publicKeyPem, DateTimeOffset expiresAt)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        var options = new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt };
        await _distributedCache.SetStringAsync(cacheKey, publicKeyPem, options);
    }

    public async Task<string?> GetPublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = KeyEncryptionCacheKeyProvider.GetPublicKeyCacheKey(tenantId, keyId);
        return await _distributedCache.GetStringAsync(cacheKey);
    }

    private static string SerializeKeyPair(RsaKeyPair keyPair)
    {
        var keyData = new
        {
            keyPair.KeyId,
            keyPair.TenantId,
            PrivateKeyPem = keyPair.PrivateKey.ExportPkcs8PrivateKeyPem(),
            PublicKeyPem = keyPair.PublicKey.ExportSubjectPublicKeyInfoPem(),
            keyPair.CreatedAt,
            keyPair.ExpiresAt,
            keyPair.IsActive
        };

        return JsonSerializer.Serialize(keyData);
    }

    private static RsaKeyPair DeserializeKeyPair(string serializedData)
    {
        using var document = JsonDocument.Parse(serializedData);
        var root = document.RootElement;
        var keyId = root.GetProperty("KeyId").GetString()!;
        var tenantId = root.GetProperty("TenantId").GetString()!;
        var privateKeyPem = root.GetProperty("PrivateKeyPem").GetString()!;
        var publicKeyPem = root.GetProperty("PublicKeyPem").GetString()!;
        var createdAt = root.GetProperty("CreatedAt").GetDateTimeOffset();
        var expiresAt = root.GetProperty("ExpiresAt").GetDateTimeOffset();
        var isActive = root.GetProperty("IsActive").GetBoolean();

        var privateKey = RSA.Create();
        privateKey.ImportFromPem(privateKeyPem);
        var publicKey = RSA.Create();
        publicKey.ImportFromPem(publicKeyPem);

        return new RsaKeyPair
        {
            KeyId = keyId,
            TenantId = tenantId,
            PrivateKey = privateKey,
            PublicKey = publicKey,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
            IsActive = isActive
        };
    }
}
