using EShop.Shared.DomainTools.EventSourcing;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class EventSourcingExtensions
{
    /// <summary>
    /// Add Event Sourcing with PostgreSQL - Complete setup for microservices
    /// </summary>
    public static IServiceCollection AddEventSourcingWithPostgreSQL<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        bool includeSnapshots = false,
        Action<EventStoreOptions>? configureOptions = null)
        where TDbContext : DbContext, IEventSourcingDbContext
    {
        ConfigureEventStoreOptions(services, configuration, includeSnapshots, configureOptions);

        services.AddScoped<IEventStoreRepository, PostgresEventStoreRepository<TDbContext>>();

        return includeSnapshots
            ? services.AddPostgresSnapshotSupport<TDbContext>()
            : services.AddScoped<IEventStoreGateway, EventStoreGateway>();
    }

    private static void ConfigureEventStoreOptions(
        IServiceCollection services,
        IConfiguration configuration,
        bool includeSnapshots,
        Action<EventStoreOptions>? configureOptions)
    {
        services.Configure<EventStoreOptions>(options =>
        {
            configuration.GetSection("EventStore").Bind(options);
            options.IncludeSnapshots = includeSnapshots;
        });

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
    }

    private static IServiceCollection AddPostgresSnapshotSupport<TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, ISnapshotDbContext
    {
        services.AddScoped<ISnapshotRepository, PostgresSnapshotRepository<TDbContext>>();
        services.AddScoped<IEventStoreGateway, EventStoreGatewayWithSnapshots>();
        return services;
    }
}