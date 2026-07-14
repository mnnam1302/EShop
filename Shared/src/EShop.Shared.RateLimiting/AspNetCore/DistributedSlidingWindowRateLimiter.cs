using EShop.Shared.RateLimiting.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.AspNetCore;

public sealed class DistributedSlidingWindowRateLimiter : RateLimiter
{
    private readonly IRateLimiter _rateLimiter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Func<SlidingWindowCheck> _checkFactory;
    private readonly string _scopeName;

    // See DistributedTokenBucketRateLimiter: a factory (not a frozen check) so a rule change takes
    // effect on the next request for a partition that ASP.NET Core keeps reusing.
    public DistributedSlidingWindowRateLimiter(
        IRateLimiter rateLimiter,
        IHttpContextAccessor httpContextAccessor,
        Func<SlidingWindowCheck> checkFactory,
        string scopeName)
    {
        _rateLimiter = rateLimiter;
        _httpContextAccessor = httpContextAccessor;
        _checkFactory = checkFactory;
        _scopeName = scopeName;
    }

    public override TimeSpan? IdleDuration => null;

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var check = _checkFactory();
        var result = await _rateLimiter.CheckSlidingWindowAsync(check, cancellationToken);

        // "Reset" for a sliding window is approximate (the window boundary, not the exact moment the
        // weighted count clears the limit) since the Lua script doesn't echo back elapsed-in-window —
        // it's an informational header, not something enforcement depends on, so this is an accepted
        // simplification rather than round-tripping more state through Redis for it.
        RateLimitHeaderWriter.Write(_httpContextAccessor.HttpContext, check.Limit, result.Remaining, (long)check.Window.TotalSeconds);

        var exceededScope = result.Allowed ? null : _scopeName;
        return new DistributedRateLimitLease(result.Allowed, result.Remaining, result.RetryAfter, exceededScope);
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        return new DistributedRateLimitLease(isAcquired: false, remaining: 0, retryAfter: TimeSpan.Zero, exceededScope: null);
    }

    public override RateLimiterStatistics? GetStatistics() => null;
}
