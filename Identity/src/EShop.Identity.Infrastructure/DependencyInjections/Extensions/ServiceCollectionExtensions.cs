using EShop.Identity.Application.Abstractions;
using EShop.Identity.Infrastructure.HashServices;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Infrastructure.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServicesInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
    }
}