using System.Security.Cryptography;

namespace EShop.Shared.Cache.KeyEncryption;

public interface IKeyManagerCachingService
{
    Task<RsaKeyPair?> TryGetActiveKeyPairAsync(string tenantId);
    Task AddActiveKeyPairAsync(string tenantId, RsaKeyPair keyPair);
    Task RemoveActiveKeyPairAsync(string tenantId);

    Task<RSA?> TryGetPublicKeyAsync(string tenantId, string keyId);
    Task AddPublicKeyAsync(string tenantId, string keyId, RSA publicKey, DateTimeOffset expiresAt);
    Task RemovePublicKeyAsync(string tenantId, string keyId);
}
