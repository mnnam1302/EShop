namespace EShop.Shared.Scoping.DependencyInjections.Options;

public record JwtOptions
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenExpiryHours { get; init; } = 1;
    public int RefreshTokenExpiryHours { get; init; } = 8;
}