using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.Contracts.Services.Order;

[ExcludeFromTopology]
public abstract class OrderIntegrationEvent : IntegrationEvent
{
}
