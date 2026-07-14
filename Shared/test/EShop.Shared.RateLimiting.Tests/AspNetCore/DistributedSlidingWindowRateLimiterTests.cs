using EShop.Shared.RateLimiting.Abstractions;
using EShop.Shared.RateLimiting.AspNetCore;
using FluentAssertions;

namespace EShop.Shared.RateLimiting.Tests.AspNetCore;

public sealed class DistributedSlidingWindowRateLimiterTests
{
    private static readonly SlidingWindowCheck OverLimitCheck = new("rl:test:ip", Limit: 5, Window: TimeSpan.FromMinutes(1));

    [Fact]
    public async Task Rejects_When_Layer_Is_Enforced_And_Over_Limit()
    {
        var (_, monitor) = EnforcementOptionsTestFactory.Create(anonymousIpEnforced: true);
        var innerLimiter = new FakeRateLimiter { SlidingWindowResult = new RateLimitResult(Allowed: false, Remaining: 0, RetryAfter: TimeSpan.FromSeconds(5)) };
        await using var limiter = new DistributedSlidingWindowRateLimiter(innerLimiter, new FakeHttpContextAccessor(), () => OverLimitCheck, "ip", "anonymous", "authorization", monitor);

        using var lease = await limiter.AcquireAsync(1);

        lease.IsAcquired.Should().BeFalse("the anonymous-IP layer is enforced and the check reported over limit");
    }

    [Fact]
    public async Task Admits_When_Layer_Is_Shadow_And_Over_Limit()
    {
        var (_, monitor) = EnforcementOptionsTestFactory.Create(anonymousIpEnforced: false);
        var innerLimiter = new FakeRateLimiter { SlidingWindowResult = new RateLimitResult(Allowed: false, Remaining: 0, RetryAfter: TimeSpan.FromSeconds(5)) };
        await using var limiter = new DistributedSlidingWindowRateLimiter(innerLimiter, new FakeHttpContextAccessor(), () => OverLimitCheck, "ip", "anonymous", "authorization", monitor);

        using var lease = await limiter.AcquireAsync(1);

        lease.IsAcquired.Should().BeTrue("the anonymous-IP layer is in shadow mode: it would have rejected, but must admit instead");
    }

    [Fact]
    public async Task Flag_Flip_Takes_Effect_On_Next_Acquire_Without_Recreating_Limiter()
    {
        var (configuration, monitor) = EnforcementOptionsTestFactory.Create(anonymousIpEnforced: false);
        var innerLimiter = new FakeRateLimiter { SlidingWindowResult = new RateLimitResult(Allowed: false, Remaining: 0, RetryAfter: TimeSpan.FromSeconds(5)) };
        await using var limiter = new DistributedSlidingWindowRateLimiter(innerLimiter, new FakeHttpContextAccessor(), () => OverLimitCheck, "ip", "anonymous", "authorization", monitor);

        using (var shadowLease = await limiter.AcquireAsync(1))
        {
            shadowLease.IsAcquired.Should().BeTrue("still in shadow mode before the flag flips");
        }

        await EnforcementOptionsTestFactory.SetFlagAsync(configuration, monitor, nameof(RateLimiterEnforcementOptions.AnonymousIpEnforced), value: true);

        using var enforcedLease = await limiter.AcquireAsync(1);

        enforcedLease.IsAcquired.Should().BeFalse("the SAME limiter instance must reject once the flag flips to enforce, with no redeploy or recreation");
    }
}
