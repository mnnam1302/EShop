using EShop.Shared.DomainTools.EventSourcing;
using EShop.Shared.DomainTools.EventSourcing.Configurations;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Sagas.AggregateSagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class EventSourcingExtensions
{
    /// <summary>
    /// Adds event sourcing services to the specified service collection without enabling snapshot support.
    /// </summary>
    /// <remarks>This method configures event sourcing using the provided database context type, but disables
    /// snapshot functionality by setting the relevant option. Use this method when snapshotting is not required for
    /// event store operations.</remarks>
    /// <typeparam name="TDbContext">The type of the Entity Framework database context that implements event store operations.</typeparam>
    /// <param name="services">The service collection to which the event sourcing services will be added.</param>
    /// <param name="configuration">The application configuration containing the EventStore settings.</param>
    /// <returns>The same service collection instance, with event sourcing services registered.</returns>
    public static IServiceCollection AddDbContextEventSourcing<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : DbContext, IEventStoreDbContext
    {
        // Configure EventStore options
        services.Configure<EventStoreOptions>(options =>
        {
            configuration.GetSection("EventStore").Bind(options);
            options.IncludeSnapshots = false;
        });

        // Register Event Store Repository and Gateway
        services.AddScoped<IEventStoreRepository, EFCoreEventStoreRepository<TDbContext>>();
        services.AddScoped<ISnapshotRepository, NullSnapshotRepository>();
        services.AddScoped<IAggregateStore, AggregateStore>();
        services.AddScoped<IAggregateSagaStore, EFCoreAggregateSagaStore>();

        return services;
    }

    /// <summary>
    /// Adds PostgreSQL-backed event sourcing with snapshot support to the specified service collection using the
    /// provided configuration.
    /// </summary>
    /// <remarks>This method registers event store and snapshot repositories using PostgreSQL as the backing
    /// store. Snapshots are enabled by default to improve event sourcing performance. Ensure that the "EventStore"
    /// section is present in the configuration for correct setup.</remarks>
    /// <typeparam name="TDbContext">The type of the Entity Framework database context to use for event sourcing. Must implement <see
    /// cref="IEventSourcingDbContext"/>.</typeparam>
    /// <param name="services">The service collection to which event sourcing and snapshot services will be added.</param>
    /// <param name="configuration">The application configuration containing the "EventStore" section used to configure event store options.</param>
    /// <param name="configureOptions">An optional delegate to further configure event store options after binding from configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
    public static IServiceCollection AddDbContextEventSourcingWithSnapshot<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EventStoreOptions>? configureOptions = null)
        where TDbContext : DbContext, IEventSourcingDbContext
    {
        services.Configure<EventStoreOptions>(options =>
        {
            configuration.GetSection("EventStore").Bind(options);
            options.IncludeSnapshots = true;
        });

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.AddScoped<IEventStoreRepository, EFCoreEventStoreRepository<TDbContext>>();
        services.AddScoped<ISnapshotRepository, EFCoreSnapshotRepository<TDbContext>>();
        services.AddScoped<IAggregateStore, AggregateStore>();
        services.AddScoped<IAggregateSagaStore, EFCoreAggregateSagaStore>();

        return services;
    }

    public static ModelBuilder AddEventStoreEntity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventStoreEntityTypeConfiguration());
        return modelBuilder;
    }

    public static ModelBuilder AddSnapshotEntity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SnapshotEntityTypeConfiguration());
        return modelBuilder;
    }
}
