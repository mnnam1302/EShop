namespace EShop.Shared.Authentication.DependencyInjections
{
    internal sealed class RsaKeyOptions
    {
        public int KeySizeInBits { get; set; } = 2048;
        public int KeyExpiryInDays { get; set; } = 7;
    }
}
