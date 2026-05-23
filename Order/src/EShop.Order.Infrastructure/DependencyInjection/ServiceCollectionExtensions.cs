using EShop.Order.Domain.Repositories;
using EShop.Order.Infrastructure.Repositories;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.JsonApi.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Order.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(environment);
        services
            .AddPostgreSqlHealthCheck(configuration, connectionString)
            .AddDbContextWithScoping<OrderDbContext>(configuration, connectionString, useRingFencedScoping: false)
            .AddRepositoryUnitOfWork();

        return services;
    }

    private static IServiceCollection AddRepositoryUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EFUnitOfWork<OrderDbContext>>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }

    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}
