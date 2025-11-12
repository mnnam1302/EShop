using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.Contracts.IntegrationEvents.Authorization;

[ExcludeFromTopology]
public interface AuthorizationEvent : IIntegrationEvent
{
}
