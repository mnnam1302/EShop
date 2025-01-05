using Eshop.Shared.DomainTools.Repositories;
using Eshop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.DependencyInjection;

namespace Eshop.Shared.DomainTools.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainTools(this IServiceCollection services)
    {
        services
            .AddRepositoryBase()
            .AddUnitOfWork();
        return services;
    }

    public static IServiceCollection AddRepositoryBase(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBaseDbContext<,,>));
        return services;
    }

    public static IServiceCollection AddRepositoryApplyAggregate(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBaseAggregateDbContext<,,>));
        return services;
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWorkDbContext<>));
        return services;
    }
}