using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order;

public sealed class MakeReservation : IntegrationCommand
{
    public required Guid OrderId { get; init; }
    public required IReadOnlyList<OrderItem> Items { get; init; }
}
