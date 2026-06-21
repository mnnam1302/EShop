using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order.Saga;

public sealed class StocksNotReserved : IntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required string FailureReason { get; init; }
}
