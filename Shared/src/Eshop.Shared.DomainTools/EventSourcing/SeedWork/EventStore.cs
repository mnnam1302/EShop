using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public class EventStore : IExcludedFromScoping
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public ulong Version { get; set; }
    public required IDomainEvent Event { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset CreatedOnUtc { get; set; }

    public static EventStore Create(IAggregate aggregate, IDomainEvent @event)
    {
        var eventStore = new EventStore
        {
            Id = Guid.NewGuid(),
            AggregateId = aggregate.Id,
            AggregateType = aggregate.GetType().Name,
            Version = @event.Version,
            EventType = @event.GetType().Name,
            Event = @event,
            CreatedOnUtc = @event.TimeStampUtc
        };

        return eventStore;
    }
}