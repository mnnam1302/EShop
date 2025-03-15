using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Tenancy.Infrastructure.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;

namespace EShop.Tenancy.Infrastructure.DependencyInjections.Extensions;

public static class RabbitMqBusFactoryConfiguratorExtensions
{
    public static void ConfigureRecieveEndpoints(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        IWebHostEnvironment environment,
        string serviceName)
    {
        bus.ConfigureEventReceiveEndpoint<FeatureEventConsumers, SupportedFeaturesUpdated>(
            context,
            environment.EnvironmentName,
            serviceName ?? "tenancy");
    }
}