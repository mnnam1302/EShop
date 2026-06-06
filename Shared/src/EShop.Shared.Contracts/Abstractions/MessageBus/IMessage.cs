using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.MessageBus;

[ExcludeFromTopology]
public interface IMessage
{
}

[ExcludeFromTopology]
public interface IAuditingMessage
{
    string TenantId { get; }
    string ActionUserId { get; }
    string ActionUserType { get; }
}
