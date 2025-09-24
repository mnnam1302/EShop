using System.Security.Cryptography;

namespace EShop.Authorization.Application.Services;

public interface IPasswordHasher
{
    string GenerateRandomPassword(int length = 12);
    string Hash(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}

public sealed class PasswordHasher : IPasswordHasher
{
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_-+=<>?";

    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Interations = 10000;

    private static HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;
    private const char Delimeter = '-';

    public string GenerateRandomPassword(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length should be at least 8 characters.", nameof(length));

        var random = new Random();
        var passwordChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            passwordChars[i] = AllowedChars[random.Next(AllowedChars.Length)];
        }

        return new string(passwordChars);
    }

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Interations, Algorithm, HashSize);

        return string.Join(Delimeter, Convert.ToHexString(hash), Convert.ToHexString(salt));
    }

    public bool VerifyHashedPassword(string password, string passwordHash)
    {
        string[] parts = passwordHash.Split(Delimeter);
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Interations, Algorithm, HashSize);
        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}
