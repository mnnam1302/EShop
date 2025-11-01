namespace EShop.Shared.Authentication.DependencyInjections;

public sealed class TenantKeyOptions
{
    public int KeySizeInBits { get; set; } = 2048;
    public int KeyExpiryInDays { get; set; } = 7;
}
