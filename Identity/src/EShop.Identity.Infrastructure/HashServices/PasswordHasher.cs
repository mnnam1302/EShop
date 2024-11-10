using EShop.Identity.Application.Abstractions;
using System.Security.Cryptography;

namespace EShop.Identity.Infrastructure.HashServices;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Interations = 10000;
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private const char Delimeter = ';';

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Interations, _hashAlgorithmName, KeySize);

        return string.Join(Delimeter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool Verify(string passwordHash, string inputPassword) // expetcted, actual
    {
        var elelments = passwordHash.Split(Delimeter);
        var salt = Convert.FromBase64String(elelments[0]);
        var hash = Convert.FromBase64String(elelments[1]);

        var hashInput = Rfc2898DeriveBytes.Pbkdf2(inputPassword, salt, Interations, _hashAlgorithmName, KeySize);
        return CryptographicOperations.FixedTimeEquals(hash, hashInput);
    }
}