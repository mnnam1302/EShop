using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.DbResourceAccessControl.Options;
using Microsoft.AspNetCore.Hosting;
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
        services.ConfigureNgSqlRetryOptionsPersistence(configuration.GetSection(nameof(NgSqlRetryOptions)));

        services.AddDbContext<DbContext, TContext>((provider, builder) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>(); // singleton
            var multiTeantConnectionInterceptor = provider.GetRequiredService<IMultiTenantConnectionInterceptor>(); // transient

            builder
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .UseLazyLoadingProxies(true)
                .UseNpgsql(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptionsAction: optionsBuilder
                        => optionsBuilder
                            .ExecutionStrategy(dependencies => new NpgsqlRetryingExecutionStrategy(
                                dependencies: dependencies,
                                maxRetryCount: options.CurrentValue.MaxRetryCount,
                                maxRetryDelay: options.CurrentValue.MaxRetryDelay,
                                errorCodesToAdd: options.CurrentValue.ErrorNumbersoAdd))
                            .MigrationsAssembly(typeof(TContext).Assembly.GetName().Name))
                .AddInterceptors(multiTeantConnectionInterceptor);
        })
            .AddMultiTenantScoping();

        //if (useRingFencedScoping)
        //{
        //    services.AddRingFencedScoping();
        //}

        return services;
    }

    private static OptionsBuilder<NgSqlRetryOptions> ConfigureNgSqlRetryOptionsPersistence(
        this IServiceCollection services,
        IConfiguration section)
    {
        return services
            .AddOptions<NgSqlRetryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}