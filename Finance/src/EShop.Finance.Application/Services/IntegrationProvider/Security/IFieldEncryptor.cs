namespace EShop.Finance.Application.Services.IntegrationProvider.Security;

public interface IFieldEncryptor
{
    string Encrypt(string plaintext);

    string Decrypt(string ciphertext);
}
