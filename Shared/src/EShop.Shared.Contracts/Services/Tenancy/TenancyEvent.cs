using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.Contracts.Services.Tenancy;

[ExcludeFromTopology]
public interface TenancyEvent : IIntegrationEvent { }