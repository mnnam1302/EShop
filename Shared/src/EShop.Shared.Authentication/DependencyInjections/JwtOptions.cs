using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Authentication.DependencyInjections;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "http://authorization";

    public string Audience { get; init; } = "http://authorization";

    [Range(1, 60)]
    public int AccessTokenExpiryMinutes { get; init; } = 15;

    [Range(1, 12)]
    public int RefreshTokenExpiryHours { get; init; } = 1;
}
