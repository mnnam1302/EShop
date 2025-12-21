using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Authentication.DependencyInjections;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "http://authorization-api";

    public string Audience { get; init; } = "http://authorization-api";

    [Range(1, 60)]
    public int AccessTokenExpiryMinutes { get; init; } = 5;

    [Range(1, 12)]
    public int RefreshTokenExpiryHours { get; init; } = 1;
}
