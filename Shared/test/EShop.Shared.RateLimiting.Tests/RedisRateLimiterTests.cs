using FluentAssertions;

namespace EShop.Shared.RateLimiting.Tests;

[Collection(nameof(RedisCollection))]
public sealed class RedisRateLimiterTests(RedisContainerFixture fixture)
{
    private static string UniqueKey(string label) => $"rl:test:{label}:{Guid.NewGuid()}";

    [Fact]
    public async Task Exact_Admission_At_The_Limit_Under_Concurrent_Bursts()
    {
        var limiter = new RedisRateLimiter(fixture.Connection);
        var key = UniqueKey("burst");

        var tasks = Enumerable.Range(0, 30)
            .Select(_ => limiter.CheckTokenBucketsAsync(
                new TokenBucketCheck(key, Capacity: 10, RefillTokensPerPeriod: 10, RefillPeriod: TimeSpan.FromHours(1)),
                null));

        var results = await Task.WhenAll(tasks);

        results.Count(r => r.Allowed).Should().Be(10);
        results.Count(r => !r.Allowed).Should().Be(20);
    }

    [Fact]
    public async Task Refill_Over_Time_Admits_Again_After_Waiting()
    {
        var limiter = new RedisRateLimiter(fixture.Connection);
        var key = UniqueKey("refill");
        var check = new TokenBucketCheck(key, Capacity: 2, RefillTokensPerPeriod: 2, RefillPeriod: TimeSpan.FromSeconds(1));

        var first = await limiter.CheckTokenBucketsAsync(check, null);
        var second = await limiter.CheckTokenBucketsAsync(check, null);
        var third = await limiter.CheckTokenBucketsAsync(check, null);

        first.Allowed.Should().BeTrue();
        second.Allowed.Should().BeTrue();
        third.Allowed.Should().BeFalse();

        await Task.Delay(TimeSpan.FromSeconds(1.2));

        var afterRefill = await limiter.CheckTokenBucketsAsync(check, null);

        afterRefill.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Cross_Replica_Correctness_Shared_Limit_Across_Two_Limiter_Instances()
    {
        var replica1 = new RedisRateLimiter(fixture.Connection);
        var replica2 = new RedisRateLimiter(fixture.Connection);
        var key = UniqueKey("cross-replica");
        var check = new TokenBucketCheck(key, Capacity: 10, RefillTokensPerPeriod: 10, RefillPeriod: TimeSpan.FromHours(1));

        var tasks = Enumerable.Range(0, 30)
            .Select(i => (i % 2 == 0 ? replica1 : replica2).CheckTokenBucketsAsync(check, null));

        var results = await Task.WhenAll(tasks);

        results.Count(r => r.Allowed).Should().Be(10);
    }

    [Fact]
    public async Task Tenant_Qualified_User_Keys_Are_Isolated_Across_Tenants()
    {
        var limiter = new RedisRateLimiter(fixture.Connection);
        var suffix = Guid.NewGuid();
        var tenantAKey = RateLimitKeyBuilder.UserBucketKey($"tenant-a-{suffix}", "user-id", "catalog");
        var tenantBKey = RateLimitKeyBuilder.UserBucketKey($"tenant-b-{suffix}", "user-id", "catalog");

        var checkA = new TokenBucketCheck(tenantAKey, Capacity: 1, RefillTokensPerPeriod: 1, RefillPeriod: TimeSpan.FromHours(1));
        var checkB = new TokenBucketCheck(tenantBKey, Capacity: 1, RefillTokensPerPeriod: 1, RefillPeriod: TimeSpan.FromHours(1));

        var drainTenantA = await limiter.CheckTokenBucketsAsync(checkA, null);
        var exhaustedTenantA = await limiter.CheckTokenBucketsAsync(checkA, null);
        var freshTenantB = await limiter.CheckTokenBucketsAsync(checkB, null);

        drainTenantA.Allowed.Should().BeTrue();
        exhaustedTenantA.Allowed.Should().BeFalse();
        freshTenantB.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Key_Expires_After_Idle_Period()
    {
        var limiter = new RedisRateLimiter(fixture.Connection);
        var key = UniqueKey("expiry");
        var check = new TokenBucketCheck(key, Capacity: 1, RefillTokensPerPeriod: 1, RefillPeriod: TimeSpan.FromSeconds(1));

        await limiter.CheckTokenBucketsAsync(check, null);

        var database = fixture.Connection.GetDatabase();
        (await database.KeyExistsAsync(key)).Should().BeTrue();

        var ttl = await database.KeyTimeToLiveAsync(key);
        ttl.Should().NotBeNull();
        ttl!.Value.Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(2));

        await Task.Delay(TimeSpan.FromSeconds(2.5));

        (await database.KeyExistsAsync(key)).Should().BeFalse();
    }
}
