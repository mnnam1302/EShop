using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Persistence.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPersistence(this IServiceCollection services)
    {
        services
            .AddDbInitializer()
            .AddRepositories()
            .AddUnitOfWorks();

        return services;
    }

    private static IServiceCollection AddDbInitializer(this IServiceCollection services)
    {
        services.AddTransient<DbInitializer>();
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IIdentityAggregateRepository<,>), typeof(IdentityAggregateRepository<,>));
        services.AddScoped(typeof(IIdentityRepositoryBase<,>), typeof(IdentityRepositoryBase<,>));
        return services;
    }

    private static IServiceCollection AddUnitOfWorks(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWorkDbContext<UsersDbContext>>();
        return services;
    }
}