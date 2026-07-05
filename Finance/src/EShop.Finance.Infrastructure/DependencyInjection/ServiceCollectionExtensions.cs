using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Http;
using EShop.Finance.Application.Services.IntegrationProvider.Security;
using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Infrastructure.Integration;
using EShop.Finance.Infrastructure.Integration.Security;
using EShop.Finance.Infrastructure.Repositories;
using EShop.Shared.Authentication.Filters;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.Diagnostics;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.Filters;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.JsonApi.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Finance.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinancePersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(environment);
        services
            .AddPostgreSqlHealthCheck(configuration, connectionString)
            .AddDbContextWithScoping<FinanceDbContext>(configuration, connectionString, useRingFencedScoping: false)
            .AddRepositoryUnitOfWork();

        return services;
    }

    private static IServiceCollection AddRepositoryUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EFUnitOfWork<FinanceDbContext>>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountingCompanyRepository, AccountingCompanyRepository>();

        return services;
    }

    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddEventBus()
            .AddMasstransitRabbitMQ(configuration)
            .AddAccountingIntegrationInfrastructure(configuration);

        services.AddSingleton(typeof(CorrelationIdLogEnrichFilter<>));

        return services;
    }

    private static IServiceCollection AddAccountingIntegrationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AesEncryptionOptions>()
            .Bind(configuration.GetSection(AesEncryptionOptions.SectionName));

        services.AddSingleton<IFieldEncryptor, AesFieldEncryptor>();
        services.AddScoped<IProviderSessionStore, ProviderSessionStore>();
        services.AddScoped<IConnectionDetailsStore, ConnectionDetailsStore>();

        // Resilient HttpClient for provider booking calls (retry + circuit breaker + timeout).
        services.AddHttpClient(OAuthAuthenticationProvider.TokenClientName);
        services.AddHttpClient(HttpIntegrationClient.HttpClientName).AddStandardResilienceHandler();

        return services;
    }

    private static IServiceCollection AddMasstransitRabbitMQ(this IServiceCollection services, IConfiguration configuration)
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
                bus.SendTopology.UseCorrelationId<OrderPaymentScheduled>(x => x.OrderId);
                bus.SendTopology.UseCorrelationId<OrderPaymentScheduleFailed>(x => x.OrderId);

                if (configuration.IsRunningInAspire())
                {
                    var connectionString = configuration.GetConnectionString("rabbitmq");
                    bus.Host(connectionString);
                }
                else
                {
                    bus.Host(massTransitConfiguration.Host, massTransitConfiguration.Port, massTransitConfiguration.VHost, h =>
                    {
                        h.Username(massTransitConfiguration.Username);
                        h.Password(massTransitConfiguration.Password);
                    });
                }

                bus.UseConsumeFilter(typeof(SystemUserContextConsumeFilter<>), context);
                bus.UseConsumeFilter(typeof(CorrelationIdLogEnrichFilter<>), context);

                bus.UseMessageRetry(retry => retry.Incremental(
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
}
