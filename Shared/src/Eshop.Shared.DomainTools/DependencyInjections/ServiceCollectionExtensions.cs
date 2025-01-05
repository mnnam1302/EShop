using Eshop.Shared.DomainTools.Aggregates;
using Eshop.Shared.DomainTools.Repositories;
using Eshop.Shared.DomainTools.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
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

    public static IServiceCollection AddRepositoryForAggregate<TDbContext, TEntity, TKey>(this IServiceCollection services)
       where TDbContext : DbContext
       where TEntity : class, IAggregateRoot<TKey>
    {
        services.AddScoped(
            typeof(IRepositoryBase<TEntity, TKey>), 
            typeof(AggregateRepositoryBaseDbContext<TDbContext, TEntity, TKey>));
        return services;
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWorkDbContext<>));
        return services;
    }
}