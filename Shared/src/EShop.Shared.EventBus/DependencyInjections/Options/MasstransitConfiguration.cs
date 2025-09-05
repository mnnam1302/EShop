namespace EShop.Shared.EventBus.DependencyInjections.Options;

public class MasstransitConfiguration
{
    public string Host { get; init; } = string.Empty;

    public string VHost { get; init; } = string.Empty;

    public ushort Port { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}