using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.AspNetCore;

public sealed class DistributedRateLimitLease : RateLimitLease
{
    private readonly long _remaining;
    private readonly TimeSpan _retryAfter;
    private readonly string? _exceededScope;

    public DistributedRateLimitLease(bool isAcquired, long remaining, TimeSpan retryAfter, string? exceededScope)
    {
        IsAcquired = isAcquired;
        _remaining = remaining;
        _retryAfter = retryAfter;
        _exceededScope = exceededScope;
    }

    public override bool IsAcquired { get; }

    public override IEnumerable<string> MetadataNames
    {
        get
        {
            yield return MetadataName.RetryAfter.Name;
            yield return RateLimitMetadataNames.Remaining.Name;
            if (_exceededScope is not null)
            {
                yield return RateLimitMetadataNames.ExceededScope.Name;
            }
        }
    }

    public override bool TryGetMetadata(string metadataName, out object? metadata)
    {
        if (metadataName == MetadataName.RetryAfter.Name)
        {
            metadata = _retryAfter;
            return true;
        }

        if (metadataName == RateLimitMetadataNames.Remaining.Name)
        {
            metadata = _remaining;
            return true;
        }

        if (metadataName == RateLimitMetadataNames.ExceededScope.Name && _exceededScope is not null)
        {
            metadata = _exceededScope;
            return true;
        }

        metadata = null;
        return false;
    }
}
