using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace EShop.Shared.Cache.KeyEncryption;

public sealed class RsaKeyPair
{
    public required string KeyId { get; init; }
    public required string TenantId { get; init; }

    [JsonIgnore]
    public RSA PrivateKey => CreateRsaFromPem(PrivateKeyPem);

    public required string PrivateKeyPem { get; init; }
    public required string PublicKeyPem { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }

    private static RSA CreateRsaFromPem(string privateKeyPem)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        return rsa;
    }

    public RSA GetPublicKey()
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(PublicKeyPem);
        return rsa;
    }
}