using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.MessageBus
{
    /// <summary>
    /// Marker for messages that should not be automatically added to topology by MassTransit when using conventions.
    /// (Keep this if your infrastructure relies on MassTransit topology conventions.)
    /// </summary>
    [ExcludeFromTopology]
    public interface IEvent : IMessage;

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
        Guid EventId { get; set; }
        ulong Version { get; set; }
        DateTimeOffset TimeStampUtc { get; set; }
    }
}