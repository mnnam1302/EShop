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
        services.ConfigureNgSqlRetryOptions(configuration.GetSection(nameof(NgSqlRetryOptions)));
        services.ConfigureNgSqlVersionOptions(configuration.GetSection(nameof(NgSqlVersionOptions)));

        // Consider DI carefully, I'd like to use AddDbContextPool, but it's scope lifetime
        services.AddDbContext<DbContext, TContext>((provider, builder) =>
        {
            var ngsqlRetryOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>();
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();
            var multiTenantConnectionInterceptor = provider.GetRequiredService<IMultiTenantIsolationStrategy>();
            //var auditableInterceptor = provider.GetRequiredService<AuditableInterceptor>();

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
                .AddInterceptors(
                    multiTenantConnectionInterceptor);
            //auditableInterceptor);
        })
            .AddMultiTenantScoping()
            .AddAuditableInterceptor();

        return services;
    }

    private static OptionsBuilder<NgSqlRetryOptions> ConfigureNgSqlRetryOptions(
        this IServiceCollection services,
        IConfiguration section)
    {
        return services
            .AddOptions<NgSqlRetryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static OptionsBuilder<NgSqlVersionOptions> ConfigureNgSqlVersionOptions(
        this IServiceCollection services,
        IConfiguration section)
    {
        return services
            .AddOptions<NgSqlVersionOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}