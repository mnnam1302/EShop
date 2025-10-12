using EShop.Catalog.Application.Shared;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.JsonConverters;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Sequences.DependencyInjections;
using MassTransit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Catalog.Application.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddCors()
            .AddSwagger()
            .AddApiVersioning()
            .AddMassTransitRabbitMQ(configuration, environment)
            .AddServiceBootstrapping();

        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services
            .AddSwaggerGenNewtonsoftSupport()
            .AddFluentValidationRulesToSwagger()
            .AddEndpointsApiExplorer()
            .AddSwaggerAPI();

        return services;
    }

    private static IServiceCollection AddApiVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options => options.ReportApiVersions = true)
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
    private static IServiceCollection AddMassTransitRabbitMQ(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var massTransitConfiguration = new MasstransitConfiguration();
        configuration.GetSection(nameof(MasstransitConfiguration)).Bind(massTransitConfiguration);

        var messageBusOptions = new MessageBusOptions();
        configuration.GetSection(nameof(MessageBusOptions)).Bind(messageBusOptions);

        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();

            cfg.AddConsumers(AssemblyReference.Assembly);

            cfg.UsingRabbitMq((context, bus) =>
            {
                bus.Host(massTransitConfiguration.Host, massTransitConfiguration.Port, massTransitConfiguration.VHost, h =>
                {
                    h.Username(massTransitConfiguration.Username);
                    h.Password(massTransitConfiguration.Password);
                });

                bus.UseMessageRetry(retry
                    => retry.Incremental(
                            retryLimit: messageBusOptions.RetryLimit,
                            initialInterval: messageBusOptions.InitialInterval,
                            intervalIncrement: messageBusOptions.IntervalIncrement));

                bus.UseNewtonsoftJsonSerializer();
                bus.ConfigureNewtonsoftJsonSerializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });
                bus.ConfigureNewtonsoftJsonDeserializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });

                bus.ConnectPublishObserver(new LoggingPublishObserver());
                bus.ConnectSendObserver(new LoggingSendObserver());
                bus.ConnectReceiveObserver(new LoggingReceiveObserver());
                bus.ConnectConsumeObserver(new LoggingConsumeObserver());

                bus.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddServiceBootstrapping(this IServiceCollection services)
    {
        services.AddTransient<DbInitializer>();

        services.AddSequenceManagement<SequenceRepository>();
        services.AddOptions<CatalogSequenceOptions>()
            .BindConfiguration(CatalogSequenceOptions.SectionName);

        services.AddScoped<IUnitOfWork, EFUnitOfWork<CatalogDbContext>>();

        services.AddScoped<IEventBusGateway, EventBusGateway>();
        services.AddScoped<IFeatureRegistrationService, CatalogFeatureRegistrationService>();
        services.AddScoped<IPermissionRegistrationService, CatalogPermissionRegistrationService>();

        return services;
    }
}
