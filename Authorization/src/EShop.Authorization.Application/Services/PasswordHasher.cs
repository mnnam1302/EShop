namespace EShop.Authorization.Application.Services;

public interface IPasswordHasher
{
    string GenerateRandomPassword(int length = 12);
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}
