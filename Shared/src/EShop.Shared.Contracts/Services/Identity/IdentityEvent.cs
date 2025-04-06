using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.Contracts.Services.Identity;

[ExcludeFromTopology]
public interface IdentityEvent : IIntegrationEvent
{
}