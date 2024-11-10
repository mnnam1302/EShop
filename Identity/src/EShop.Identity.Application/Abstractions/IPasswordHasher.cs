namespace EShop.Identity.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string inputPassword);

    bool Verify(string passwordHash, string inputPassword);
}