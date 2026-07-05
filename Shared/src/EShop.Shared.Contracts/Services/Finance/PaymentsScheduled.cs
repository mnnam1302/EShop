using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Finance;

public sealed class PaymentsScheduled : IntegrationEvent
{
    public required Guid AccountId { get; init; }
    public required Guid OrderId { get; init; }
}
