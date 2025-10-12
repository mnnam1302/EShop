using EShop.Authorization.Application.Services;
using EShop.Authorization.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Authorization.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationApplication(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRootOrganizationService, RootOrganizationService>();
        return services;
    }
}
