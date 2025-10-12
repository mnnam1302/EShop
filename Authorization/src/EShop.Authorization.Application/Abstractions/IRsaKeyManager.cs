using EShop.Shared.Cache.KeyEncryption;
using System.Security.Cryptography;

namespace EShop.Authorization.Application.Abstractions;

public interface IRsaKeyManager
{
    /// <summary>
    /// Generates a new RSA key pair for the specified tenant.
    /// </summary>
    Task<RsaKeyPair> GenerateKeyPairAsync(string tenantId);

    /// <summary>
    /// Gets the currently active RSA key pair for JWT token signing.
    /// </summary>
    Task<RsaKeyPair?> GetActiveKeyPairAsync(string tenantId);

    /// <summary>
    /// Ensures a valid key pair exists for the tenant, creating one if necessary.
    /// </summary>
    Task EnsureValidKeyPairExistsAsync(string tenantId);

    /// <summary>
    /// Gets the public key for JWT token validation.
    /// </summary>
    Task<RSA> GetPublicKeyAsync(string tenantId, string keyId);

    /// <summary>
    /// Rotates keys by generating a new active key pair while preserving the old one for validation.
    /// </summary>
    Task RotateKeysAsync(string tenantId);
}
