using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.Redis;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace EShop.Shared.RateLimiting.Tests;

// Owns a dedicated Redis container (not the shared RedisCollection fixture) because this test stops
// and restarts Redis mid-run to simulate an outage — that must not affect other tests running in
// parallel against the shared container.
public sealed class FailOpenRateLimiterTests : IAsyncLifetime
{
    private RedisContainer _redisContainer = null!;
    private IConnectionMultiplexer _connection = null!;
    private FailOpenRateLimiter _limiter = null!;

    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder().WithImage("redis:7-alpine").Build();
        await _redisContainer.StartAsync();

        _connection = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        var inner = new RedisRateLimiter(_connection);
        _limiter = new FailOpenRateLimiter(inner, NullLogger<FailOpenRateLimiter>.Instance);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task Admits_Via_InMemory_Fallback_During_Redis_Outage_And_Recovers_Without_Restart()
    {
        // Capacity is deliberately well above the loop count below: the in-memory fallback is a
        // fresh, separate bucket from the Redis-side one, so this test must not conflate "did the
        // fallback's own capacity get exhausted" with "did fail-open kick in".
        var check = new TokenBucketCheck(
            $"rl:test:failopen:{Guid.NewGuid()}",
            Capacity: 50,
            RefillTokensPerPeriod: 50,
            RefillPeriod: TimeSpan.FromHours(1));

        var beforeOutage = await _limiter.CheckTokenBucketsAsync(check, null);
        beforeOutage.Allowed.Should().BeTrue("Redis is healthy at this point");

        await _redisContainer.StopAsync();

        // Drive enough failed calls to trip the circuit breaker (5 allowed failures), then confirm
        // requests keep being admitted (fail-open) rather than throwing or being denied outright.
        for (var i = 0; i < 6; i++)
        {
            var duringOutage = await _limiter.CheckTokenBucketsAsync(check, null);
            duringOutage.Allowed.Should().BeTrue($"call {i} during the outage should fail open, not throw or deny");
        }

        await _redisContainer.StartAsync();

        // The circuit breaker's break duration must elapse before it attempts to close again.
        await Task.Delay(TimeSpan.FromSeconds(16));

        var afterRecoveryKey = $"rl:test:failopen-recovery:{Guid.NewGuid()}";
        var recoveryCheck = new TokenBucketCheck(afterRecoveryKey, Capacity: 50, RefillTokensPerPeriod: 50, RefillPeriod: TimeSpan.FromHours(1));

        var afterRecovery = await _limiter.CheckTokenBucketsAsync(recoveryCheck, null);
        afterRecovery.Allowed.Should().BeTrue();

        // If the check went through Redis (not the in-memory fallback) the key now exists there.
        var database = _connection.GetDatabase();
        (await database.KeyExistsAsync(afterRecoveryKey)).Should().BeTrue("recovery means checks are served by Redis again, not the fallback");
    }
}
