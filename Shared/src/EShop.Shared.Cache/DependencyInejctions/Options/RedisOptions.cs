namespace EShop.Shared.Cache.DependencyInejctions.Options;

public record RedisOptions
{
    public bool Enabled { get; init; } = true;
    public string ConnectionString { get; init; } = string.Empty;
}