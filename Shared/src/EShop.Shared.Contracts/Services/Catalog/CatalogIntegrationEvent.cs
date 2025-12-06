using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;
namespace EShop.Shared.Contracts.Services.Catalog;

[ExcludeFromTopology]
public abstract class CatalogIntegrationEvent : IntegrationEvent
{
}
