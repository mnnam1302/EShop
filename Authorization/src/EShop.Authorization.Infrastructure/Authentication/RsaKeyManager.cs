using EShop.Authorization.Application.Abstractions;
using EShop.Shared.Cache.KeyEncryption;
using EShop.Shared.Scoping.Exceptions;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace EShop.Authorization.Infrastructure.Authentication;

public sealed class RsaKeyManager : IRsaKeyManager
{
    private const int KeySizeInBits = 2048;
    private const int KeyValidityDays = 30;

    private readonly ILogger<RsaKeyManager> _logger;
    private readonly IKeyManagerCachingService _keyManagerCaching;

    public RsaKeyManager(ILogger<RsaKeyManager> logger, IKeyManagerCachingService keyManagerCaching)
    {
        _logger = logger;
        _keyManagerCaching = keyManagerCaching;
    }

    public async Task<RsaKeyPair> GenerateKeyPairAsync(string tenantId)
    {
        var keyPair = CreateRsaKeyPair(tenantId);
        await StoreKeyPairAsync(keyPair);

        _logger.LogInformation("Generated new RSA key pair for tenant {TenantId} with KeyId {KeyId}", tenantId, keyPair.KeyId);

        return keyPair;
    }

    private static RsaKeyPair CreateRsaKeyPair(string tenantId)
    {
        using var rsa = RSA.Create(KeySizeInBits);

        var keyId = Guid.NewGuid().ToString();
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddDays(KeyValidityDays);

        // Export as PEM strings instead of RSA instances
        var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
        var publicKeyPem = rsa.ExportRSAPublicKeyPem();

        return new RsaKeyPair
        {
            KeyId = keyId,
            TenantId = tenantId,
            PrivateKeyPem = privateKeyPem,
            PublicKeyPem = publicKeyPem,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        };
    }

    private async Task StoreKeyPairAsync(RsaKeyPair keyPair)
    {
        await _keyManagerCaching.AddActiveKeyPairAsync(keyPair.TenantId, keyPair);

        // Store public key separately using the GetPublicKey() method
        var publicKey = keyPair.GetPublicKey();
        await _keyManagerCaching.AddPublicKeyAsync(keyPair.TenantId, keyPair.KeyId, publicKey, keyPair.ExpiresAt);

        // Dispose the temporary RSA instance
        publicKey.Dispose();
    }

    public async Task<RsaKeyPair?> GetActiveKeyPairAsync(string tenantId)
    {
        return await _keyManagerCaching.TryGetActiveKeyPairAsync(tenantId);
    }

    public async Task EnsureValidKeyPairExistsAsync(string tenantId)
    {
        var existingKeyPair = await GetActiveKeyPairAsync(tenantId);
        if (existingKeyPair is null || existingKeyPair.ExpiresAt <= DateTimeOffset.UtcNow.AddDays(7))
        {
            await GenerateKeyPairAsync(tenantId);
        }
    }

    public async Task<RSA> GetPublicKeyAsync(string tenantId, string keyId)
    {
        var publicKey = await _keyManagerCaching.TryGetPublicKeyAsync(tenantId, keyId);

        return publicKey ?? throw new NotFoundException($"Public key not found for tenant {tenantId} and keyId {keyId}");
    }

    public async Task RotateKeysAsync(string tenantId)
    {
        _logger.LogInformation("Rotating RSA keys for tenant {TenantId}", tenantId);

        await RemoveExistingKeysAsync(tenantId);
        await GenerateKeyPairAsync(tenantId);
    }

    private async Task RemoveExistingKeysAsync(string tenantId)
    {
        var existingKeyPair = await GetActiveKeyPairAsync(tenantId);

        await _keyManagerCaching.RemoveActiveKeyPairAsync(tenantId);

        if (existingKeyPair != null)
        {
            await _keyManagerCaching.RemovePublicKeyAsync(tenantId, existingKeyPair.KeyId);
        }
    }
}
