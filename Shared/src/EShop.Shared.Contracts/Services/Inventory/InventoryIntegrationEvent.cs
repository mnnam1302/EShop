using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.Contracts.Services.Inventory;

[ExcludeFromTopology]
public abstract class InventoryIntegrationEvent : IntegrationEvent
{
}
