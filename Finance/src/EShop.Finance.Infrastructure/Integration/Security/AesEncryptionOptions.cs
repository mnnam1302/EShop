namespace EShop.Finance.Infrastructure.Integration.Security;

public sealed class AesEncryptionOptions
{
    public const string SectionName = "Encryption";

    public string Key { get; set; } = string.Empty;
}
