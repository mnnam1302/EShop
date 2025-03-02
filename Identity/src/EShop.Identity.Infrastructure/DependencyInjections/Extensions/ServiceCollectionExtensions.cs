using EShop.Identity.Application.Abstractions;
using EShop.Identity.Infrastructure.Authentication;
using EShop.Identity.Infrastructure.HashServices;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Infrastructure.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddServices();
        return services;
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ITokenService, TokenService>();
    }
}