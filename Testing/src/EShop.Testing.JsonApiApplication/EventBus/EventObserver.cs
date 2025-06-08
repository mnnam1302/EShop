using MassTransit;

namespace EShop.Testing.JsonApiApplication.EventBus;

public class EventObserver<TEvent> : IObserver<ConsumeContext<TEvent>>
    where TEvent : class
{
    private readonly IIntegrationEventsTracker _tracker;

    public EventObserver(IIntegrationEventsTracker tracker)
    {
        _tracker = tracker;
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(ConsumeContext<TEvent> value)
    {
        _tracker.Track(value.Message);
    }
}