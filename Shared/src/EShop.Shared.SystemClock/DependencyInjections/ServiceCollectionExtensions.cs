using Microsoft.Extensions.DependencyInjection;
using EShop.Shared.SystemClock.Abstractions;

namespace EShop.Shared.SystemClock.DependencyInjections
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSystemClock(this IServiceCollection services)
        {
            // Register the SystemClock as a singleton service
            services.AddSingleton<ISystemClock, SystemClock>();
            return services;
        }
    }
}
