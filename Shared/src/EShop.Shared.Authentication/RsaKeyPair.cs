namespace EShop.Shared.Authentication
{
    public sealed class RsaKeyPair
    {
        public required string KeyId { get; init; }
        public required string TenantId { get; init; }

        public required string PrivateKeyPem { get; init; }
        public required string PublicKeyPem { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
    }
}
