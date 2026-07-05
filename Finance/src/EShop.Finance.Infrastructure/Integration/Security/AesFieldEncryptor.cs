using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using EShop.Finance.Application.Services.IntegrationProvider.Security;
using Microsoft.Extensions.Options;

namespace EShop.Finance.Infrastructure.Integration.Security;

public sealed class AesFieldEncryptor : IFieldEncryptor
{
    private const int IvSizeInBytes = 16;
    private readonly byte[] _encryptionKey;

    public AesFieldEncryptor(IOptions<AesEncryptionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _encryptionKey = Convert.FromBase64String(options.Value.Key);
        if (_encryptionKey.Length is not (16 or 24 or 32))
        {
            throw new ArgumentException("AES key must be 128, 192, or 256 bits.", nameof(options));
        }
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        Span<byte> iv = stackalloc byte[IvSizeInBytes];
        RandomNumberGenerator.Fill(iv);

        var plaintextBytesCount = Encoding.UTF8.GetByteCount(plaintext);
        var ciphertextBytesCount = aes.GetCiphertextLengthCbc(plaintextBytesCount);
        var totalPayloadLength = IvSizeInBytes + ciphertextBytesCount;

        byte[] plaintextBuffer = ArrayPool<byte>.Shared.Rent(plaintextBytesCount);
        byte[] cryptographicPayload = new byte[totalPayloadLength];

        try
        {
            Encoding.UTF8.GetBytes(plaintext, plaintextBuffer.AsSpan(0, plaintextBytesCount));

            Span<byte> ivDestination = cryptographicPayload.AsSpan(0, IvSizeInBytes);
            Span<byte> ciphertextDestination = cryptographicPayload.AsSpan(IvSizeInBytes, ciphertextBytesCount);

            iv.CopyTo(ivDestination);

            if (!aes.TryEncryptCbc(
                    plaintextBuffer.AsSpan(0, plaintextBytesCount),
                    iv,
                    ciphertextDestination,
                    out var bytesWritten))
            {
                throw new CryptographicException("Encryption failed due to insufficient destination buffer size.");
            }

            var finalLength = IvSizeInBytes + bytesWritten;
            return Convert.ToBase64String(cryptographicPayload.AsSpan(0, finalLength));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintextBuffer.AsSpan(0, plaintextBytesCount));
            ArrayPool<byte>.Shared.Return(plaintextBuffer);
        }
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;

        byte[] cryptographicPayload = Convert.FromBase64String(ciphertext);
        if (cryptographicPayload.Length <= IvSizeInBytes)
        {
            throw new FormatException("Ciphertext is too short to contain a valid initialization vector.");
        }

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;

        ReadOnlySpan<byte> iv = cryptographicPayload.AsSpan(0, IvSizeInBytes);
        ReadOnlySpan<byte> ciphertextBytes = cryptographicPayload.AsSpan(IvSizeInBytes);

        byte[] plaintextBuffer = ArrayPool<byte>.Shared.Rent(ciphertext.Length);
        try
        {
            if (!aes.TryDecryptCbc(ciphertextBytes, iv, plaintextBuffer, out var bytesWritten))
            {
                throw new CryptographicException("Decryption failed due to insufficient destination buffer size.");
            }

            return Encoding.UTF8.GetString(plaintextBuffer.AsSpan(0, bytesWritten));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintextBuffer.AsSpan(0, ciphertext.Length));
            ArrayPool<byte>.Shared.Return(plaintextBuffer);
        }
    }
}
