using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EShop.Testing.JsonApiApplication.EventBus;

public interface IIntegrationEventsTracker
{
    void Track(object eventMessage);

    void TrackConsumerCompleted(object eventMessage);

    void ClearPublishedEvents();

    IReadOnlyList<PublishedEvent> GetPublishedEvents();

    /// <summary>
    /// Waits until all tracked events have been consumed, or the timeout is reached.
    /// </summary>
    Task WaitForConsumersAsync(TimeSpan? timeout = null);
}

public class IntegrationEventsTracker : IIntegrationEventsTracker
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentBag<PublishedEvent> publishedEvents = [];
    private readonly ILogger<IntegrationEventsTracker> _logger;

    private int _trackedCount;
    private int _completedCount;
    private readonly SemaphoreSlim _completionSignal = new(0);

    public IntegrationEventsTracker(ILogger<IntegrationEventsTracker> logger)
    {
        _logger = logger;
    }

    public void Track(object eventMessage)
    {
        publishedEvents.Add(new PublishedEvent(eventMessage));
        Interlocked.Increment(ref _trackedCount);
        _logger.LogDebug(
            "Tracking Integration events - '{eventTypeName}' (hashcode={eventHashCode})",
            eventMessage.GetType().Name,
            eventMessage.GetHashCode());
    }

    public void TrackConsumerCompleted(object eventMessage)
    {
        var completed = Interlocked.Increment(ref _completedCount);
        _logger.LogDebug(
            "Consumer completed for Integration event - '{eventTypeName}' ({completed}/{tracked})",
            eventMessage.GetType().Name,
            completed,
            Volatile.Read(ref _trackedCount));

        _completionSignal.Release();
    }

    public async Task WaitForConsumersAsync(TimeSpan? timeout = null)
    {
        var effectiveTimeout = timeout ?? DefaultTimeout;
        var tracked = Volatile.Read(ref _trackedCount);
        var completed = Volatile.Read(ref _completedCount);

        if (tracked == 0 || completed >= tracked)
        {
            return;
        }

        var deadline = DateTime.UtcNow + effectiveTimeout;

        while (Volatile.Read(ref _completedCount) < Volatile.Read(ref _trackedCount))
        {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                _logger.LogWarning(
                    "Timed out waiting for consumers. Completed {completed}/{tracked} in {timeout}",
                    Volatile.Read(ref _completedCount),
                    Volatile.Read(ref _trackedCount),
                    effectiveTimeout);
                break;
            }

            await _completionSignal.WaitAsync(remaining);
        }
    }

    public void ClearPublishedEvents()
    {
        publishedEvents.Clear();
        Interlocked.Exchange(ref _trackedCount, 0);
        Interlocked.Exchange(ref _completedCount, 0);

        // Drain any leftover semaphore counts
        while (_completionSignal.CurrentCount > 0)
        {
            _completionSignal.Wait(0);
        }

        _logger.LogDebug("Clearing Integration events - cleared items");
    }

    public IReadOnlyList<PublishedEvent> GetPublishedEvents()
    {
        return publishedEvents.ToList();
    }
}