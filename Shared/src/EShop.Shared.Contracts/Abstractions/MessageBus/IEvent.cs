using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.MessageBus;

[ExcludeFromTopology]
public interface IEvent : IMessage
{
    Guid EventId { get; }
    DateTimeOffset TimeStampUtc { get; }
}

[ExcludeFromTopology]
public interface IIntegrationEvent : IEvent
{
    string TenantId { get; }
    string ActionUserId { get; }
    string ActionUserType { get; }
}

[ExcludeFromTopology]
public interface IDomainEvent : IEvent
{
    ulong Version { get; set; }
}

[ExcludeFromTopology]
public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset TimeStampUtc { get; } = DateTimeOffset.UtcNow;
    public required string TenantId { get; init; }
    public required string ActionUserId { get; init; }
    public required string ActionUserType { get; init; }
}