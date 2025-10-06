using System.Security.Cryptography;

namespace EShop.Shared.Cache.KeyEncryption;

public sealed record RsaKeyPair
{
    public required string KeyId { get; init; }
    public required string TenantId { get; init; }
    public required RSA PrivateKey { get; init; }
    public required RSA PublicKey { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public bool IsActive { get; init; } = true;
}