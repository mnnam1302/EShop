using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.AspNetCore;
using FluentAssertions;

namespace EShop.Shared.RateLimiting.Tests.AspNetCore;

public sealed class DistributedTokenBucketRateLimiterTests
{
    private static readonly TokenBucketCheck OverLimitCheck = new("rl:test:tenant", Capacity: 10, RefillTokensPerPeriod: 10, RefillPeriod: TimeSpan.FromMinutes(1));

    [Fact]
    public async Task Rejects_When_Layer_Is_Enforced_And_Over_Limit()
    {
        var (_, monitor) = EnforcementOptionsTestFactory.Create(tenantEnforced: true);
        var innerLimiter = new FakeRateLimiter
        {
            TokenBucketResult = new CombinedRateLimitResult(
                new RateLimitResult(Allowed: false, Remaining: 0, RetryAfter: TimeSpan.FromSeconds(5)),
                null)
        };
        await using var limiter = new DistributedTokenBucketRateLimiter(
            innerLimiter,
            new FakeHttpContextAccessor(),
            primaryFactory: () => OverLimitCheck,
            primaryScopeName: "tenant",
            tenantId: "tenant-1",
            domain: "tenancy",
            monitor);

        using var lease = await limiter.AcquireAsync(1);

        lease.IsAcquired.Should().BeFalse("the tenant layer is enforced and the check reported over limit");
    }

    [Fact]
    public async Task Admits_When_Layer_Is_Shadow_And_Over_Limit()
    {
        var (_, monitor) = EnforcementOptionsTestFactory.Create(tenantEnforced: false);
        var innerLimiter = new FakeRateLimiter
        {
            TokenBucketResult = new CombinedRateLimitResult(
                new RateLimitResult(Allowed: false, Remaining: 0, RetryAfter: TimeSpan.FromSeconds(5)),
                null)
        };
        await using var limiter = new DistributedTokenBucketRateLimiter(
            innerLimiter,
            new FakeHttpContextAccessor(),
            primaryFactory: () => OverLimitCheck,
            primaryScopeName: "tenant",
            tenantId: "tenant-1",
            domain: "tenancy",
            monitor);

        using var lease = await limiter.AcquireAsync(1);

        lease.IsAcquired.Should().BeTrue("the tenant layer is in shadow mode: it would have rejected, but must admit instead");
    }

    [Fact]
    public async Task Flag_Flip_Takes_Effect_On_Next_Acquire_Without_Recreating_Limiter()
    {
        var (configuration, monitor) = EnforcementOptionsTestFactory.Create(tenantEnforced: false);
        var innerLimiter = new FakeRateLimiter
        {
            TokenBucketResult = new CombinedRateLimitResult(
                new RateLimitResult(Allowed: false, Remaining: 0, RetryAfter: TimeSpan.FromSeconds(5)),
                null)
        };
        await using var limiter = new DistributedTokenBucketRateLimiter(
            innerLimiter,
            new FakeHttpContextAccessor(),
            primaryFactory: () => OverLimitCheck,
            primaryScopeName: "tenant",
            tenantId: "tenant-1",
            domain: "tenancy",
            monitor);

        using (var shadowLease = await limiter.AcquireAsync(1))
        {
            shadowLease.IsAcquired.Should().BeTrue("still in shadow mode before the flag flips");
        }

        await EnforcementOptionsTestFactory.SetFlagAsync(configuration, monitor, nameof(RateLimiterEnforcementOptions.TenantEnforced), value: true);

        using var enforcedLease = await limiter.AcquireAsync(1);

        enforcedLease.IsAcquired.Should().BeFalse("the SAME limiter instance must reject once the flag flips to enforce, with no redeploy or recreation");
    }
}
