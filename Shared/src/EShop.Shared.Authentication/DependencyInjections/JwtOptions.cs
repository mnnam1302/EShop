using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.Authentication.DependencyInjections;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = "http://authorizationService";

    public string Audience { get; init; } = "http://authorizationService";

    [Range(1, 60)]
    public int AccessTokenExpiryMinutes { get; init; } = 5;

    [Range(1, 12)]
    public int RefreshTokenExpiryHours { get; init; } = 1;
}
