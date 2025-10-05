using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Scoping.DependencyInjections.Options;

public sealed class JwtOptions
{
    public const string ConfigurationSection = "JwtOptions";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    [Range(1, 60)]
    public int AccessTokenExpiryMinutes { get; init; }

    [Range(1, 12)]
    public int RefreshTokenExpiryHours { get; init; }
}