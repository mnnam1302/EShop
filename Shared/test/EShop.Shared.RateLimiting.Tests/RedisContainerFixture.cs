using StackExchange.Redis;
using Testcontainers.Redis;

namespace EShop.Shared.RateLimiting.Tests;

public sealed class RedisContainerFixture : IAsyncLifetime
{
    private RedisContainer _container = null!;

    public IConnectionMultiplexer Connection { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        await _container.StartAsync();
        Connection = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await Connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(RedisCollection))]
public sealed class RedisCollection : ICollectionFixture<RedisContainerFixture>
{
}
