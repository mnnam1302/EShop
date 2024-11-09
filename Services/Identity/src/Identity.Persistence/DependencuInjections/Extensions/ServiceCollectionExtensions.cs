using Identity.Persistence.DependencuInjections.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Identity.Persistence.DependencuInjections.Extensions;

public static class ServiceCollectionExtensions
{
    #region SqlServer
    public static void AddSqlPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DbContext, UserDbContext>((provider, builder) =>
        {
            //var auditableInterceptor = provider.GetService<UpdateAuditableEntitiesInterceptor>();
            //var domainEventsInterceptor = provider.GetService<ConvertDomainEventsToEventsInterceptor>();
            var options = provider.GetRequiredService<IOptionsMonitor<SqlServerRetryOptions>>();

            builder
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .UseLazyLoadingProxies(true)
                .UseSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: optionsBuilder
                        => optionsBuilder
                        .ExecutionStrategy(
                            dependencies => new SqlServerRetryingExecutionStrategy(
                            dependencies: dependencies,
                            maxRetryCount: options.CurrentValue.MaxRetryCount,
                            maxRetryDelay: options.CurrentValue.MaxRetryDelay,
                            errorNumbersToAdd: options.CurrentValue.ErrorNumbersoAdd))
                        .MigrationsAssembly(typeof(UserDbContext).Assembly.GetName().Name));
                //.AddInterceptors(
                //    auditableInterceptor,
                //    domainEventsInterceptor);
            });
    }

    public static OptionsBuilder<SqlServerRetryOptions> ConfigureSqlServerRetryOptionsPersistence(
        this IServiceCollection services,
        IConfiguration section)
    {
        return services
            .AddOptions<SqlServerRetryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    #endregion

    #region Postgres
    public static void AddNqSqlPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DbContext, UserDbContext>((provider, builder) =>
        {
            //var auditableInterceptor = provider.GetService<UpdateAuditableEntitiesInterceptor>();
            //var domainEventsInterceptor = provider.GetService<ConvertDomainEventsToEventsInterceptor>();
            var options = provider.GetRequiredService<IOptionsMonitor<NgSqlRetryOptions>>();

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
                            .MigrationsAssembly(typeof(UserDbContext).Assembly.GetName().Name));
                //.AddInterceptors(
                //    auditableInterceptor,
                //    domainEventsInterceptor);
            });
    }

    public static OptionsBuilder<NgSqlRetryOptions> ConfigureNgSqlRetryOptionsPersistence(
        this IServiceCollection services,
        IConfiguration section)
    {
        return services
            .AddOptions<NgSqlRetryOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    #endregion

    public static void AddDbInitialize(this IServiceCollection services)
    {
        //services.AddTransient<DbInitializer>();
    }

    public static void AddInterceptorPersistence(this IServiceCollection services)
    {
        //services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
        //services.AddSingleton<ConvertDomainEventsToEventsInterceptor>();
    }

    public static void AddRepositoryPersistence(this IServiceCollection services)
    {
        //services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
        //services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
    }
}