using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Authentication.DependencyInjections;

public sealed class TenantKeyOptions
{
    public int KeySizeInBits { get; set; } = 2048;
    public int KeyExpiryInDays { get; set; } = 7;

    /// <summary>
    /// TTL in minutes for the previous RSA key after rotation.
    /// Should be at least as long as the access token expiry to allow
    /// tokens signed with the previous key to remain valid until they expire.
    /// </summary>
    [Range(1, 60)]
    public int PreviousKeyTtlMinutes { get; set; } = 15;
}
