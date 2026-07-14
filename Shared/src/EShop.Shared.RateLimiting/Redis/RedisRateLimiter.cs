using EShop.Shared.Diagnostics;
using EShop.Shared.RateLimiting.Abstractions;
using StackExchange.Redis;
using System.Diagnostics;

namespace EShop.Shared.RateLimiting.Redis;

public sealed class RedisRateLimiter : IRateLimiter
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly string _tokenBucketScript;
    private readonly string _slidingWindowScript;

    public RedisRateLimiter(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _tokenBucketScript = RateLimitScriptLoader.Load("token_bucket.lua");
        _slidingWindowScript = RateLimitScriptLoader.Load("sliding_window.lua");
    }

    public async Task<CombinedRateLimitResult> CheckTokenBucketsAsync(
        TokenBucketCheck primary,
        TokenBucketCheck? secondary,
        CancellationToken cancellationToken = default)
    {
        var database = _connectionMultiplexer.GetDatabase();

        RedisKey[] keys;
        RedisValue[] values;

        if (secondary is null)
        {
            keys = [primary.Key];
            values = BuildTokenBucketArgs(primary);
        }
        else
        {
            keys = [primary.Key, secondary.Key];
            values = [.. BuildTokenBucketArgs(primary), .. BuildTokenBucketArgs(secondary)];
        }

        var stopwatch = Stopwatch.StartNew();
        var reply = (RedisResult[])(await database.ScriptEvaluateAsync(_tokenBucketScript, keys, values))!;
        RateLimiterMetrics.RedisLatency.Record(stopwatch.Elapsed.TotalMilliseconds);

        var primaryResult = ParseResult(reply, 0);
        var secondaryResult = secondary is null ? null : ParseResult(reply, 1);

        return new CombinedRateLimitResult(primaryResult, secondaryResult);
    }

    private static RedisValue[] BuildTokenBucketArgs(TokenBucketCheck check)
    {
        return [check.Capacity, check.RefillTokensPerPeriod, (long)check.RefillPeriod.TotalSeconds];
    }

    private static RateLimitResult ParseResult(RedisResult[] reply, int index)
    {
        var offset = index * 3;

        var allowed = (long)reply[offset] == 1;
        var remaining = (long)reply[offset + 1];
        var retryAfterMs = (long)reply[offset + 2];

        return new RateLimitResult(allowed, remaining, TimeSpan.FromMilliseconds(retryAfterMs));
    }

    public async Task<RateLimitResult> CheckSlidingWindowAsync(
        SlidingWindowCheck check,
        CancellationToken cancellationToken = default)
    {
        var database = _connectionMultiplexer.GetDatabase();

        RedisKey[] keys = [check.Key];
        RedisValue[] values = [check.Limit, (long)check.Window.TotalSeconds];

        var stopwatch = Stopwatch.StartNew();
        var reply = (RedisResult[])(await database.ScriptEvaluateAsync(_slidingWindowScript, keys, values))!;
        RateLimiterMetrics.RedisLatency.Record(stopwatch.Elapsed.TotalMilliseconds);

        return ParseResult(reply, 0);
    }
}
