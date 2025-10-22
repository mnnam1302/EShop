namespace EShop.Shared.DomainTools;

public static class Jitterer
{
    private const int JitterUpperLimit = 100;

    private static readonly Random jitterer = new Random();

    public static TimeSpan GetJitteredDelay(int scaleMultiplierFromMs = 1, int jitterUpperLimit = JitterUpperLimit, int minimum = 0)
    {
        return TimeSpan.FromMilliseconds(minimum * scaleMultiplierFromMs + jitterer.Next(0, jitterUpperLimit) * scaleMultiplierFromMs);
    }

    public static TimeSpan GetJitteredDelay(TimeSpan fromInclusive, TimeSpan toExclusive, int scaleMultiplierFromMs = 1)
    {
        if (fromInclusive > toExclusive)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fromInclusive),
                $"{nameof(fromInclusive)} must be less than or equal to {nameof(toExclusive)}.");
        }

        double fromMs = fromInclusive.TotalMilliseconds;

        var delta = toExclusive.TotalMilliseconds - fromMs;
        long jitteredDeltaAsLong = (long)(jitterer.NextDouble() * delta);
        long jitteredDeltaWithScale = jitteredDeltaAsLong / scaleMultiplierFromMs * scaleMultiplierFromMs;

        return TimeSpan.FromMilliseconds(fromMs + jitteredDeltaWithScale);
    }
}