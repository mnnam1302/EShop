using EShop.Identity.Domain.Repositories;
using EShop.Identity.Persistence.Repositories;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Persistence.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityPersistence(this IServiceCollection services)
    {
        services
            .AddRepositories()
            .AddUnitOfWorks();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped(typeof(IIdentityRepositoryBase<,>), typeof(IdentityRepositoryBase<,>));
        return services;
    }

    private static IServiceCollection AddUnitOfWorks(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWorkDbContext<UsersDbContext>>();
        return services;
    }
}