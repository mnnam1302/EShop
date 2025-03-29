using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.MessageBus;

[ExcludeFromTopology]
public interface IEvent : IMessage
{
}

[ExcludeFromTopology]
public interface IDomainEvent : IEvent
{
    //long Version { get; }
}

[ExcludeFromTopology]
public interface IIntegrationEvent : IEvent
{
    string TenantId { get; }
    string ActionUserId { get; }
    string ActionUserType { get; }
}