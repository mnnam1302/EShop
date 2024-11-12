namespace EShop.Identity.Infrastructure.DependencyInjections.Options;

public class RedisOptions
{
    public bool Enabled { get; init; }
    public string ConnectionString { get; init; }
}