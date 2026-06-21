using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.EventBus;
using System.Text.Json;

namespace EShop.Inventory.Infrastructure.Services;

internal sealed class OutboxWriter(InventoryDbContext dbContext) : IOutboxWriter
{
    public void Enqueue<TEvent>(TEvent @event) where TEvent : class
    {
        var outbox = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            AggregateName = typeof(TEvent).Assembly.GetName().Name ?? string.Empty,
            AggregateId = string.Empty,
            EventId = Guid.NewGuid().ToString(),
            EventName = typeof(TEvent).FullName ?? typeof(TEvent).Name,
            Payload = JsonSerializer.Serialize(@event),
            OccurredOnUtc = DateTimeOffset.UtcNow
        };

        dbContext.OutboxMessages.Add(outbox);
    }
}
