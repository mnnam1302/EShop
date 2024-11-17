namespace ApiGateway.DependencyInjections.Options;

public record JwtOptions
{
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public string SecretKey { get; init; }
    public int AcceessExpireHour { get; init; }
    public int RefreshExpireHour { get; init; }
}