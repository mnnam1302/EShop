namespace EShop.Shared.EventBus;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public string AggregateId { get; set; } = string.Empty;

    public string AggregateName { get; set; } = string.Empty;

    public string EventId { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset OccurredOnUtc { get; set; }

    public DateTimeOffset? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }
}