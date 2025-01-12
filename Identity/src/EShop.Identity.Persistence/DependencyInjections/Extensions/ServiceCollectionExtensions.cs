
using Eshop.Shared.DomainTools.UnitOfWorks;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Abstractions.UnitOfWorks;
using EShop.Identity.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Persistence.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServicesPersistenceLayer(this IServiceCollection services)
    {
        services
            .AddDbInitializer()
            .AddRepositoryAndUnitOfWork()
            .AddRepositories();

        return services;
    }

    private static IServiceCollection AddDbInitializer(this IServiceCollection services)
    {
        services.AddTransient<DbInitializer>();
        return services;
    }

    private static IServiceCollection AddRepositoryAndUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped(typeof(Domain.Abstractions.UnitOfWorks.IUnitOfWork), typeof(UnitOfWork));
        services.AddScoped(typeof(IRepositoryBase<,>), typeof(RepositoryBase<,>));
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IIdentityRepository<,>), typeof(IdentityRepository<,>));
        services.AddScoped<Eshop.Shared.DomainTools.UnitOfWorks.IUnitOfWork, UnitOfWorkDbContext<UsersDbContext>>();
        return services;
    }

    //private static void AddInterceptorPersistence(this IServiceCollection services)
    //{
    //    services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
    //    services.AddSingleton<ConvertDomainEventsToEventsInterceptor>();
    //}
}