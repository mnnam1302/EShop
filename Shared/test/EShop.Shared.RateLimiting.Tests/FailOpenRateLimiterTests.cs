using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.Redis;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using System.Diagnostics;
using Testcontainers.Redis;

namespace EShop.Shared.RateLimiting.Tests;

// Owns a dedicated Redis container (not the shared RedisCollection fixture) because this test
// simulates an outage mid-run, which must not affect other tests sharing that container.
//
// The outage is simulated via `docker pause`/`unpause`, not by stopping/restarting the container:
// on Docker Desktop's WSL2 backend, restarting the same container can leave the host-side port
// forward broken indefinitely (confirmed with a raw PingAsync that never reconnected within 2
// minutes, no rate-limiter code involved). Pausing freezes the container without tearing down its
// network namespace, so the client just sees an unresponsive server and recovers instantly on
// unpause.
public sealed class FailOpenRateLimiterTests : IAsyncLifetime
{
    // Mirrors PollyPolicies' rate-limiter circuit breaker config, so this test provably exercises
    // "circuit open" behavior (not just isolated timeouts) if that config ever changes.
    private const int FailuresToTripCircuitBreaker = 5;
    private const int CallsDuringOutage = FailuresToTripCircuitBreaker + 1;
    private const int CircuitBreakerBreakDurationSeconds = 15;

    private const int FallbackBucketCapacity = 50;

    private RedisContainer _redisContainer = null!;
    private IConnectionMultiplexer _connection = null!;
    private FailOpenRateLimiter _limiter = null!;

    public async Task InitializeAsync()
    {
        _redisContainer = new RedisBuilder().WithImage("redis:7-alpine").Build();
        await _redisContainer.StartAsync();

        _connection = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        _limiter = new FailOpenRateLimiter(new RedisRateLimiter(_connection), NullLogger<FailOpenRateLimiter>.Instance);
    }

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task Admits_Via_InMemory_Fallback_During_Redis_Outage_And_Recovers_Without_Restart()
    {
        var outageCheck = NewCheck($"rl:test:failopen:outage:{Guid.NewGuid()}");

        var beforeOutage = await _limiter.CheckTokenBucketsAsync(outageCheck, null);
        beforeOutage.Allowed.Should().BeTrue("Redis is healthy at this point");

        await PauseRedisAsync();

        for (var i = 0; i < CallsDuringOutage; i++)
        {
            var duringOutage = await _limiter.CheckTokenBucketsAsync(outageCheck, null);
            duringOutage.Allowed.Should().BeTrue($"call {i} during the outage should fail open, not throw or deny");
        }

        await UnpauseRedisAsync();

        // The circuit breaker's break duration must elapse before it gives Redis another chance.
        await Task.Delay(TimeSpan.FromSeconds(CircuitBreakerBreakDurationSeconds + 1));

        var recoveryKey = $"rl:test:failopen:recovery:{Guid.NewGuid()}";
        var afterRecovery = await _limiter.CheckTokenBucketsAsync(NewCheck(recoveryKey), null);
        afterRecovery.Allowed.Should().BeTrue();

        // If the check went through Redis (not the in-memory fallback) the key now exists there.
        var database = _connection.GetDatabase();
        (await database.KeyExistsAsync(recoveryKey)).Should().BeTrue("recovery means checks are served by Redis again, not the fallback");
    }

    private static TokenBucketCheck NewCheck(string key) =>
        new(key, Capacity: FallbackBucketCapacity, RefillTokensPerPeriod: FallbackBucketCapacity, RefillPeriod: TimeSpan.FromHours(1));

    private Task PauseRedisAsync() => RunDockerAsync($"pause {_redisContainer.Id}");

    private Task UnpauseRedisAsync() => RunDockerAsync($"unpause {_redisContainer.Id}");

    private static async Task RunDockerAsync(string arguments)
    {
        var psi = new ProcessStartInfo("docker", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync();
    }
}
