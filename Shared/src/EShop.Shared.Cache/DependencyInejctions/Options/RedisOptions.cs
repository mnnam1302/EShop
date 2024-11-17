namespace EShop.Shared.Cache.DependencyInejctions.Options;

public record RedisOptions
{
    public bool Enabled { get; init; }
    public string ConnectionString { get; init; }
}