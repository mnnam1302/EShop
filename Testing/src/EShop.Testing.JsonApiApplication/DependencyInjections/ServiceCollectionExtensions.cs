using EShop.Shared.JsonApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Testing.JsonApiApplication.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlTestDbContext<TContext>(
        this IServiceCollection services,
        PostgreSqlTestDatabase testDatabase,
        Action<IServiceProvider, DbContextOptionsBuilder>? additionalDbContextConfig = null)
        where TContext : DbContext
    {
        services.AddSingleton(testDatabase);
        services.AddTransient<ITestDatabaseConnectionInterceptor, PostgreSqlTestDatabaseConnectionInterceptor>();

        services.AddMultiTenantScoping();

        services.AddDbContext<TContext>((provider, builder) =>
            ConfigurePostgreSqlTestDbContext(provider, builder, additionalDbContextConfig));

        return services;
    }

    private static void ConfigurePostgreSqlTestDbContext(
        IServiceProvider sp,
        DbContextOptionsBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder>? additionalDbContextConfig)
    {
        builder.EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .UseNpgsql();

        var testDatabaseConnectionInterceptor = sp.GetRequiredService<ITestDatabaseConnectionInterceptor>();
        builder.AddInterceptors(testDatabaseConnectionInterceptor);

        additionalDbContextConfig?.Invoke(sp, builder);
    }

    public static IServiceCollection AddPostgreSqlTestDbContextFactory<TContext>(
        this IServiceCollection services,
        PostgreSqlTestDatabase testDatabase,
        Action<IServiceProvider, DbContextOptionsBuilder>? additionalDbContextConfig = null)
        where TContext : DbContext
    {
        services.AddSingleton(testDatabase);
        services.AddTransient<ITestDatabaseConnectionInterceptor, PostgreSqlTestDatabaseConnectionInterceptor>();

        services.AddDbContextFactory<TContext>((provider, builder) =>
        {
            ConfigurePostgreSqlTestDbContext(provider, builder, additionalDbContextConfig);
        })
            .AddMultiTenantScoping();

        return services;
    }
}