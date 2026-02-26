using EShop.Shared.SystemClock.Abstractions;

namespace EShop.Shared.SystemClock;

internal sealed class SystemClock : ISystemClock
{
    public DateTimeOffset? CustomUtcNow { get; private set; }
    public TimeSpan? NowOffset { get; private set; }

    public DateTimeOffset GetUtcNow() => GetUtcNowWithOffset();

    private DateTimeOffset GetUtcNowWithOffset()
    {
        var utcNow = CustomUtcNow ?? DateTimeOffset.UtcNow;
        if (NowOffset.HasValue)
        {
            utcNow = utcNow.Add(NowOffset.Value);
        }

        return utcNow;
    }

    public Task SetUtcNow(DateTimeOffset customDatetime)
    {
        CustomUtcNow = customDatetime;
        return Task.CompletedTask;
    }
}