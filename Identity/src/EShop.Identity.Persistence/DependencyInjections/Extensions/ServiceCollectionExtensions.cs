using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Abstractions.UnitOfWorks;
using EShop.Identity.Persistence.DependencyInjections.Options;
using EShop.Identity.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace EShop.Identity.Persistence.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddNqSqlPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<DbContext, UsersDbContext>((provider, builder) =>
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
                            .MigrationsAssembly(typeof(UsersDbContext).Assembly.GetName().Name));
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

    public static void AddInterceptorPersistence(this IServiceCollection services)
    {
        //services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
        //services.AddSingleton<ConvertDomainEventsToEventsInterceptor>();
    }

    public static void AddRepositoryAndUnitOfWorkPersistence(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
        services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
    }
}