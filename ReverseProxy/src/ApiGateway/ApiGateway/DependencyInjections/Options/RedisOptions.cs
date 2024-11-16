namespace ApiGateway.DependencyInjections.Options;

public class RedisOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; }
}