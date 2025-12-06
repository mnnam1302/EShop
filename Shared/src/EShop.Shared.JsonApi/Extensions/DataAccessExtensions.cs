using EShop.Shared.DbResourceAccessControl.Extensions;
using EShop.Shared.DbResourceAccessControl.Interceptors;
using EShop.Shared.DbResourceAccessControl.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace EShop.Shared.JsonApi.Extensions;

public static class DataAccessExtensions
{
    private static readonly string[] tags = ["db", "postgresql", "sql"];

    public static IServiceCollection AddPostgreSqlHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
                name: "postgresql",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: tags);

        return services;
    }

    public static IServiceCollection AddDbContextWithScoping<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useRingFencedScoping = false)
        where TContext : DbContext
    {
        services.AddDatabaseOptions(configuration);
        services.AddMultiTenantScoping();
        services.AddDomainEventsDispatcherInterceptor();

        services.AddDbContext<DbContext, TContext>((provider, builder) =>
        {
            var ngsqlRetryOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>();
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();
            var tenantIsolationStrategy = provider.GetRequiredService<IMultiTenantIsolationStrategy>();
            var domainEventsDispatcherInterceptor = provider.GetRequiredService<IDispatchDomainEventsInterceptor>();

            builder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .UseLazyLoadingProxies()
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
                .AddInterceptors(tenantIsolationStrategy, domainEventsDispatcherInterceptor);
        });

        return services;
    }

    public static IServiceCollection AddDbContextPoolWithScoping<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useRingFencedScoping = false)
        where TDbContext : DbContext
    {
        services.AddDatabaseOptions(configuration);
        services.AddMultiTenantScoping();

        if (IsDesignTime())
        {
            services.AddDesignTimeDbContext<TDbContext>(configuration);
        }
        else
        {
            services.AddRuntimeDbContext<TDbContext>(configuration);
        }

        return services;
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

    private static void AddDesignTimeDbContext<TDbContext>(this IServiceCollection services, IConfiguration configuration)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>((provider, builder) =>
        {
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();

            builder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
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
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .UseLazyLoadingProxies()
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
}