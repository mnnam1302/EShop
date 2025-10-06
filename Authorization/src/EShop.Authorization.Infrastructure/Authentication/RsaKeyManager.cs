using EShop.Authorization.Application.Abstractions;
using EShop.Shared.Cache.KeyEncryption;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace EShop.Authorization.Infrastructure.Authentication;

internal sealed class RsaKeyManager : IRsaKeyManager
{
    private const int KeySizeInBits = 2048;
    private const int KeyValidityDays = 30;

    private readonly IKeyManagerCachingService _keyManagerCaching;
    private readonly ILogger<RsaKeyManager> _logger;

    public RsaKeyManager(IKeyManagerCachingService keyManagerCaching, ILogger<RsaKeyManager> logger)
    {
        _keyManagerCaching = keyManagerCaching;
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

        keyPair.PrivateKey.ImportParameters(rsa.ExportParameters(true));
        keyPair.PublicKey.ImportParameters(rsa.ExportParameters(false));

        await _keyManagerCaching.AddActiveKeyPairAsync(tenantId, keyPair);
        await _keyManagerCaching.AddPublicKeyAsync(tenantId, keyId, keyPair.PublicKey, expiresAt);

        _logger.LogInformation("Generated new RSA key pair for tenant {TenantId} with KeyId {KeyId}", tenantId, keyId);

        return keyPair;
    }

    public async Task<RsaKeyPair?> GetActiveKeyPairAsync(string tenantId)
    {
        return await _keyManagerCaching.TryGetActiveKeyPairAsync(tenantId);
    }

    public async Task<RSA> GetPublicKeyAsync(string tenantId, string keyId)
    {
        var publicKey = await _keyManagerCaching.TryGetPublicKeyAsync(tenantId, keyId);

        if (publicKey != null)
        {
            return publicKey;
        }

        throw new NotFoundException($"Public key not found for tenant {tenantId} and keyId {keyId}");
    }

    public async Task RotateKeysAsync(string tenantId)
    {
        _logger.LogInformation("Rotating RSA keys for tenant {TenantId}", tenantId);

        // 1. Remove old keys
        await _keyManagerCaching.RemoveActiveKeyPairAsync(tenantId);

        // 2. Generate new keys
        await GenerateKeyPairAsync(tenantId);
    }
}
