namespace EShop.Identity.Infrastructure.DependencyInjections.Options;

public record MasstransitConfiguration
{
    public string Host { get; init; }

    public string VHost { get; init; }

    public ushort Port { get; init; }

    public string Username { get; init; }

    public string Password { get; init; }
}