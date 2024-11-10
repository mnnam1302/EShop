using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.MessageBus;

[ExcludeFromTopology]
public interface IEvent : IMessage
{
}

[ExcludeFromTopology]
public interface IIntegrationEvent : IEvent
{
    string TenantId { get; }
    string ActionUserId { get; }
    string ActionUserType { get; }
}

[ExcludeFromTopology]
public interface IDelayedEvent : IEvent { }

[ExcludeFromTopology]
public interface IVersionedEvent : IEvent
{
    long Version { get; }
}

[ExcludeFromTopology]
public interface IDomainEvent : IVersionedEvent { }