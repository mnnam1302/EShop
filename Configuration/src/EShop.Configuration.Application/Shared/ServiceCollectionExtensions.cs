using EShop.Shared.DbResourceAccessControl.Interceptors;
using EShop.Shared.DbResourceAccessControl.Options;
using EShop.Shared.JsonApi.DependencyInjections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace EShop.Configuration.Application.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseOptions(configuration);
        services.AddDatabaseContext(configuration);

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

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        if (IsDesignTime())
        {
            AddDesignTimeDbContext(services, configuration);
        }
        else
        {
            AddRuntimeDbContext(services, configuration);
        }

        return services;
    }

    private static void AddDesignTimeDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ConfigurationDbContext>((provider, builder) =>
        {
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();

            builder
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    ConfigureNpgsqlOptions(ngsqlVersionOptions.CurrentValue));
        });
    }

    private static void AddRuntimeDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<ConfigurationDbContext>((provider, builder) =>
        {
            var ngsqlRetryOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>();
            var ngsqlVersionOptions = provider.GetRequiredService<IOptionsMonitor<NgSqlVersionOptions>>();
            var multiTenantConnectionInterceptor = provider.GetRequiredService<IMultiTenantIsolationStrategy>();

            builder
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .UseLazyLoadingProxies(true)
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    ConfigureNpgsqlOptionsWithRetry(ngsqlVersionOptions.CurrentValue, ngsqlRetryOptions.CurrentValue))
                .AddInterceptors(multiTenantConnectionInterceptor);
        })
        .AddMultiTenantScopingV2();
    }

    private static Action<NpgsqlDbContextOptionsBuilder> ConfigureNpgsqlOptions(NgSqlVersionOptions versionOptions)
    {
        return optionsBuilder => optionsBuilder
            .SetPostgresVersion(versionOptions.Major, versionOptions.Minor)
            .MigrationsAssembly(AssemblyReference.Assembly.GetName().Name);
    }

    private static Action<NpgsqlDbContextOptionsBuilder> ConfigureNpgsqlOptionsWithRetry(
        NgSqlVersionOptions versionOptions,
        NgSqlRetryOptions retryOptions)
    {
        return optionsBuilder => optionsBuilder
            .SetPostgresVersion(versionOptions.Major, versionOptions.Minor)
            .ExecutionStrategy(dependencies => new NpgsqlRetryingExecutionStrategy(
                dependencies: dependencies,
                maxRetryCount: retryOptions.MaxRetryCount,
                maxRetryDelay: retryOptions.MaxRetryDelay,
                errorCodesToAdd: retryOptions.ErrorNumbersoAdd))
            .MigrationsAssembly(AssemblyReference.Assembly.GetName().Name);
    }

    private static bool IsDesignTime()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Any(assembly => assembly.FullName?.StartsWith("Microsoft.EntityFrameworkCore.Design") == true);
    }
}