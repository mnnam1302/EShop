using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace EShop.Testing.JsonApiApplication.EventBus;

public interface IIntegrationEventsTracker
{
    void Track(object eventMessage);
}

public class IntegrationEventsTracker : IIntegrationEventsTracker
{
    private readonly ConcurrentBag<PublishedEvent> publishedEvents = [];
    private readonly ILogger<IntegrationEventsTracker> _logger;

    public IntegrationEventsTracker(ILogger<IntegrationEventsTracker> logger)
    {
        _logger = logger;
    }

    public void Track(object eventMessage)
    {
        publishedEvents.Add(new PublishedEvent(eventMessage));
        _logger.LogDebug(
                "Tracking Integration events - '{eventTypeName}' (hashcode={eventHashCode})",
                eventMessage.GetType().Name,
                eventMessage.GetHashCode());
    }
}