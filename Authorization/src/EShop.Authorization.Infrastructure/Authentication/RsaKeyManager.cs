using EShop.Authorization.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace EShop.Authorization.Infrastructure.Authentication;

internal sealed class RsaKeyManager : IRsaKeyManager
{
    private const int KeySizeInBits = 2048;
    private const int KeyValidityDays = 30;

    private readonly IDistributedCache _cache;
    private readonly ILogger<RsaKeyManager> _logger;

    public RsaKeyManager(IDistributedCache cache, ILogger<RsaKeyManager> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<RsaKeyPair> GenerateKeyPairAsync(string tenantId)
    {
        using var rsa = RSA.Create(KeySizeInBits);

        var keyId = Guid.NewGuid().ToString();
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(KeyValidityDays);

        var keyPair = new RsaKeyPair
        {
            KeyId = keyId,
            TenantId = tenantId,
            PrivateKey = RSA.Create(),
            PublicKey = RSA.Create(),
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        };

        // Copy the key parameters
        keyPair.PrivateKey.ImportParameters(rsa.ExportParameters(true));
        keyPair.PublicKey.ImportParameters(rsa.ExportParameters(false));

        // Store in cache/database
        await StoreKeyPairAsync(keyPair);

        _logger.LogInformation("Generated new RSA key pair for tenant {TenantId} with KeyId {KeyId}",
            tenantId, keyId);

        return keyPair;
    }

    private async Task StoreKeyPairAsync(RsaKeyPair keyPair)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = keyPair.ExpiresAt
        };

        var activeKeyCache = $"rsa_keypair_active_{keyPair.TenantId}";
        var publicKeyCache = $"rsa_publickey_{keyPair.TenantId}_{keyPair.KeyId}";

        var serializedKeyPair = SerializeKeyPair(keyPair);
        var publicKeyPem = keyPair.PublicKey.ExportSubjectPublicKeyInfoPem();

        await _cache.SetStringAsync(activeKeyCache, serializedKeyPair, options);
        await _cache.SetStringAsync(publicKeyCache, publicKeyPem, options);
    }

    public async Task<RsaKeyPair?> GetActiveKeyPairAsync(string tenantId)
    {
        var cacheKey = $"rsa_keypair_active_{tenantId}";
        var cachedKeyPair = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedKeyPair))
        {
            return DeserializeKeyPair(cachedKeyPair);
        }

        return null;
    }

    public async Task<RSA> GetPublicKeyAsync(string tenantId, string keyId)
    {
        var cacheKey = $"rsa_publickey_{tenantId}_{keyId}";
        var cachedPublicKey = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedPublicKey))
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(cachedPublicKey);
            return rsa;
        }

        throw new KeyNotFoundException($"Public key not found for tenant {tenantId} and keyId {keyId}");
    }

    public async Task RotateKeysAsync(string tenantId)
    {
        _logger.LogInformation("Rotating RSA keys for tenant {TenantId}", tenantId);

        // Generate new key pair
        await GenerateKeyPairAsync(tenantId);

        // Keep old keys for a grace period to validate existing tokens
        // Implementation depends on your key storage strategy
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
