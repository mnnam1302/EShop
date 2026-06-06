using MassTransit;

namespace EShop.Shared.Contracts.Abstractions.MessageBus;

[ExcludeFromTopology]
public interface IIntegrationCommand : IMessage, IAuditingMessage
{
}

/// <summary>
/// Asynchronous command bus via message queue
/// </summary>
[ExcludeFromTopology]
public abstract class IntegrationCommand : IIntegrationCommand
{
    public required string TenantId { get; init; }
    public required string ActionUserId { get; init; }
    public required string ActionUserType { get; init; }
}
