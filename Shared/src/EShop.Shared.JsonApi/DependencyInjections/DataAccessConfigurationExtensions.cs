using EShop.Shared.DbResourceAccessControl.Interceptors;
using EShop.Shared.DbResourceAccessControl.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class DataAccessConfigurationExtensions
{
    public static IServiceCollection AddPostgreSqlHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
                name: "postgresql",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: new[] { "db", "postgresql", "sql" });

        return services;
    }

    /// <summary>
    /// Registers the specified <see cref="TContext"/> type with standard configuration. Also registers necessary
    /// services for tenant isolation and optional ring-fenced scoping.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="useRingFencedScoping">Indicates whether ring-fencing is enabled for the created
    /// <see cref="TContext"/> instances.</param>
    /// <param name="additionalDbContextConfig">Additional configuration for the <see cref="TContext"/>.</param>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddDbContextWithScoping<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useRingFencedScoping = false)
        where TContext : DbContext
    {
        services.AddDatabaseOptions(configuration);

        // Consider DI carefully, I'd like to use AddDbContextPool, but it's scope lifetime
        services.AddDbContext<DbContext, TContext>((provider, builder) =>
        {
            var ngsqlRetryOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>();
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();
            var multiTenantConnectionInterceptor = provider.GetRequiredService<IMultiTenantIsolationStrategy>();

            builder
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .UseLazyLoadingProxies(true)
                .UseNpgsql(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptionsAction: optionsBuilder
                        => optionsBuilder
                            .SetPostgresVersion(ngsqlVersionOptions.CurrentValue.Major, ngsqlVersionOptions.CurrentValue.Minor)
                            .ExecutionStrategy(dependencies => new NpgsqlRetryingExecutionStrategy(
                                dependencies: dependencies,
                                maxRetryCount: ngsqlRetryOptions.CurrentValue.MaxRetryCount,
                                maxRetryDelay: ngsqlRetryOptions.CurrentValue.MaxRetryDelay,
                                errorCodesToAdd: ngsqlRetryOptions.CurrentValue.ErrorNumbersoAdd))
                            .MigrationsAssembly(typeof(TContext).Assembly.GetName().Name))
                .AddInterceptors(multiTenantConnectionInterceptor);
        })
            .AddMultiTenantScoping()
            .AddAuditableInterceptor();

        return services;
    }

    /// <summary>
    /// Adds a pooled <see cref="DbContext"/> of type <typeparamref name="TContext"/> to the service collection,  with
    /// optional support for ring-fenced scoping and multi-tenant configurations.
    /// </summary>
    /// <remarks>This method configures the <see cref="DbContext"/> differently depending on whether the
    /// application is running  in design-time or runtime mode. Design-time mode adds support for tools like migrations,
    /// while runtime mode  configures the <see cref="DbContext"/> for normal application usage.</remarks>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/> to be added.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the <see cref="DbContext"/> is added.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance used to configure the <see cref="DbContext"/>.</param>
    /// <param name="useRingFencedScoping">A boolean value indicating whether to enable ring-fenced scoping for the <see cref="DbContext"/>.  If <see
    /// langword="true"/>, the <see cref="DbContext"/> will be scoped to specific tenants; otherwise,  it will use the
    /// default scoping behavior.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance with the <see cref="DbContext"/> and related
    /// configurations added.</returns>
    public static IServiceCollection AddDbContextPoolWithScoping<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useRingFencedScoping = false)
        where TContext : DbContext
    {
        services
            .AddDatabaseOptions(configuration)
            .AddMultiTenantScopingV2();

        if (IsDesignTime())
        {
            services.AddDesignTimeDbContext<TContext>(configuration);
        }
        else
        {
            services.AddRuntimeDbContext<TContext>(configuration);
        }

        return services;
    }

    private static void AddDesignTimeDbContext<TDbContext>(this IServiceCollection services, IConfiguration configuration)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>((provider, builder) =>
        {
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();

            builder
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptionsAction: optionsBuilder => optionsBuilder
                        .SetPostgresVersion(ngsqlVersionOptions.CurrentValue.Major, ngsqlVersionOptions.CurrentValue.Minor)
                        .MigrationsAssembly(typeof(TDbContext).Assembly.GetName().Name));
        });
    }

    private static void AddRuntimeDbContext<TDbContext>(this IServiceCollection services, IConfiguration configuration)
        where TDbContext : DbContext
    {
        services.AddDbContextPool<TDbContext>((provider, builder) =>
        {
            var ngsqlRetryOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>();
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();
            var multiTenantConnectionInterceptor = provider.GetRequiredService<IMultiTenantIsolationStrategy>();

            builder
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .UseLazyLoadingProxies(true)
                .UseNpgsql(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptionsAction: optionsBuilder => optionsBuilder
                        .SetPostgresVersion(ngsqlVersionOptions.CurrentValue.Major, ngsqlVersionOptions.CurrentValue.Minor)
                        .ExecutionStrategy(dependencies => new NpgsqlRetryingExecutionStrategy(
                            dependencies: dependencies,
                            maxRetryCount: ngsqlRetryOptions.CurrentValue.MaxRetryCount,
                            maxRetryDelay: ngsqlRetryOptions.CurrentValue.MaxRetryDelay,
                            errorCodesToAdd: ngsqlRetryOptions.CurrentValue.ErrorNumbersoAdd))
                        .MigrationsAssembly(typeof(TDbContext).Assembly.GetName().Name))
                .AddInterceptors(multiTenantConnectionInterceptor);
        });
    }

    private static IServiceCollection AddDatabaseOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<NgSqlRetryOptions>()
            .Bind(configuration.GetSection(nameof(NgSqlRetryOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<NgSqlVersionOptions>()
            .Bind(configuration.GetSection(nameof(NgSqlVersionOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static bool IsDesignTime()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Any(assembly => assembly.FullName?.StartsWith("Microsoft.EntityFrameworkCore.Design") == true);
    }
}