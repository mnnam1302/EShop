using EShop.Shared.DbResourceAccessControl.Extensions;
using EShop.Shared.DbResourceAccessControl.Interceptors;
using EShop.Shared.DbResourceAccessControl.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace EShop.Shared.JsonApi.Extensions;

public static class DataAccessExtensions
{
    public static IServiceCollection AddPostgreSqlHealthCheck(this IServiceCollection services, IConfiguration configuration, string connectionString)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(
                connectionString: connectionString,
                name: "postgresql",
                failureStatus: HealthStatus.Degraded,
                tags: ["db", "postgresql", "sql"]);

        return services;
    }

    public static IServiceCollection AddDbContextWithScoping<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString,
        bool useRingFencedScoping = false,
         Action<IServiceProvider, DbContextOptionsBuilder>? additionalDbContextConfig = null)
        where TContext : DbContext
    {
        services.AddDatabaseOptions(configuration);
        services.AddMultiTenantScoping();
        services.AddDomainEventsDispatcherInterceptor();

        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            var retryOptions = serviceProvider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>();
            var versionOptions = serviceProvider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();

            var tenantIsolationStrategy = serviceProvider.GetRequiredService<IMultiTenantIsolationStrategy>();
            var domainEventsDispatcherInterceptor = serviceProvider.GetRequiredService<IDispatchDomainEventsInterceptor>();

            options
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .UseLazyLoadingProxies()
                .UseNpgsql(
                    //connectionString: configuration.GetConnectionString("DefaultConnection"),
                    connectionString: connectionString,
                    npgsqlOptionsAction: optionsBuilder => optionsBuilder
                        .SetPostgresVersion(versionOptions.CurrentValue.Major, versionOptions.CurrentValue.Minor)
                        .ExecutionStrategy(dependencies => new NpgsqlRetryingExecutionStrategy(
                            dependencies: dependencies,
                            maxRetryCount: retryOptions.CurrentValue.MaxRetryCount,
                            maxRetryDelay: retryOptions.CurrentValue.MaxRetryDelay,
                            errorCodesToAdd: retryOptions.CurrentValue.ErrorNumbersoAdd))
                        .MigrationsAssembly(typeof(TContext).Assembly.GetName().Name))
                .AddInterceptors(tenantIsolationStrategy, domainEventsDispatcherInterceptor);

            additionalDbContextConfig?.Invoke(serviceProvider, options);
        });

        if (useRingFencedScoping)
        {
            services.AddRingFencedScoping();
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
}