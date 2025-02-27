using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Tenancy.Persistence;

namespace EShop.Tenancy.API.DependencyInjections.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services
                .AddDbContextWithScoping<TenancyDbContext>(configuration);

            return services;
        }

        public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddUserPermissionsProvider(configuration);

            services.AddServicesApiLayer();

            return services;
        }

        private static void AddServicesApiLayer(this IServiceCollection services)
        {
            services.AddSingleton<ExceptionHandlingMiddleware>();
        }
    }
}