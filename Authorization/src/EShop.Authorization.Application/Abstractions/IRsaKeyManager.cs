using EShop.Shared.Cache.KeyEncryption;
using System.Security.Cryptography;

namespace EShop.Authorization.Application.Abstractions;

public interface IRsaKeyManager
{
    Task<RsaKeyPair> GenerateKeyPairAsync(string tenantId);
    Task<RsaKeyPair?> GetActiveKeyPairAsync(string tenantId);
    Task<RSA> GetPublicKeyAsync(string tenantId, string keyId);
    Task RotateKeysAsync(string tenantId);
}
