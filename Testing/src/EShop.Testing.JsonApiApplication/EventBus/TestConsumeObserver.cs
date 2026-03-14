using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Testing.JsonApiApplication.EventBus;

/// <summary>
/// MassTransit consume observer that tracks all consumer activity.
/// Enables reliable waiting for consumers to complete instead of using hardcoded delays.
/// Handles cascading consumers (consumer A triggers event → consumer B picks it up).
/// </summary>
public sealed class TestConsumeObserver : IConsumeObserver
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan QuietPeriod = TimeSpan.FromMilliseconds(200);

    private readonly ILogger<TestConsumeObserver> _logger;

    private int _activeConsumers;
    private readonly SemaphoreSlim _consumerCompleted = new(0);

    public TestConsumeObserver(ILogger<TestConsumeObserver> logger)
    {
        _logger = logger;
    }

    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        var count = Interlocked.Increment(ref _activeConsumers);
        _logger.LogDebug(
            "Consumer starting for {MessageType} (active={ActiveCount})",
            typeof(T).Name,
            count);
        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        var count = Interlocked.Decrement(ref _activeConsumers);
        _logger.LogDebug(
            "Consumer completed for {MessageType} (active={ActiveCount})",
            typeof(T).Name,
            count);
        _consumerCompleted.Release();
        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        var count = Interlocked.Decrement(ref _activeConsumers);
        _logger.LogWarning(
            exception,
            "Consumer faulted for {MessageType} (active={ActiveCount})",
            typeof(T).Name,
            count);
        _consumerCompleted.Release();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Waits until all consumer activity has settled: no active consumers and no new consumer fires
    /// for the quiet period duration. This handles cascading consumers reliably.
    /// </summary>
    public async Task WaitForQuietAsync(TimeSpan? timeout = null)
    {
        var effectiveTimeout = timeout ?? DefaultTimeout;
        var deadline = DateTime.UtcNow + effectiveTimeout;

        while (true)
        {
            // If all consumers are done, wait for the quiet period to see if more are triggered
            if (Volatile.Read(ref _activeConsumers) <= 0)
            {
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    return;
                }

                var waitTime = remaining < QuietPeriod ? remaining : QuietPeriod;
                var signaled = await _consumerCompleted.WaitAsync(waitTime);

                if (!signaled && Volatile.Read(ref _activeConsumers) <= 0)
                {
                    // No new consumer activity during quiet period — we're done
                    return;
                }
            }
            else
            {
                // Consumers still active — wait for one to complete
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero)
                {
                    _logger.LogWarning(
                        "Timed out waiting for consumers to settle. Active={ActiveCount}",
                        Volatile.Read(ref _activeConsumers));
                    return;
                }

                await _consumerCompleted.WaitAsync(remaining);
            }
        }
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _activeConsumers, 0);

        // Drain semaphore
        while (_consumerCompleted.CurrentCount > 0)
        {
            _consumerCompleted.Wait(0);
        }
    }
}
