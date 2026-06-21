using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.EventBus;
using Newtonsoft.Json;

namespace EShop.Inventory.Infrastructure.Services;

internal sealed class OutboxWriter(InventoryDbContext dbContext) : IOutboxWriter
{
    public void ConvertDomainEventsToOutboxMessages<TAggregate>(string aggregateId, TAggregate aggregate) where TAggregate : IAggregateRoot
    {
        if (aggregate is null)
        {
            throw new ArgumentNullException(nameof(aggregate));
        }

        var domainEvents = aggregate.GetDomainEvents();
        aggregate.ClearDomainEvents();

        var events = domainEvents
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                AggregateName = aggregate.GetType().Name,
                EventId = domainEvent.EventId.ToString(),
                EventName = domainEvent.GetType().Name,
                Payload = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    }),
                OccurredOnUtc = DateTime.UtcNow,
            })
            .ToList();

        dbContext.OutboxMessages.AddRange(events);
    }
}
