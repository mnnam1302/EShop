namespace EShop.Shared.Cache.DependencyInejctions.Options;

public sealed record RedisOptions
{
    public bool Enabled { get; init; } = true;

    public string Host { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ConnectionString => string.IsNullOrEmpty(Password) ? Host : $"{Host},password={Password}";
}