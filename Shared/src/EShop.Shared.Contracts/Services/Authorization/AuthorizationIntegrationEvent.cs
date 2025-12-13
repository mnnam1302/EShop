using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.Contracts.Services.Authorization;

[ExcludeFromTopology]
public abstract class AuthorizationIntegrationEvent : IntegrationEvent
{
}
