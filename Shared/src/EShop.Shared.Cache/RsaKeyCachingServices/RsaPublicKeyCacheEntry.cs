namespace EShop.Shared.Cache.KeyEncryption;

/// <summary>
/// Cache entry for storing RSA public keys as PEM strings.
/// This allows JSON serialization while maintaining security practices.
/// </summary>
public sealed class RsaPublicKeyCacheEntry
{
    public required string KeyId { get; init; }
    public required string TenantId { get; init; }
    public required string PublicKeyPem { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
