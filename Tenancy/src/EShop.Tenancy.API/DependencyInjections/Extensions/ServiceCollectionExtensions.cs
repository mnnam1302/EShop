using EShop.Tenancy.Persistence;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.JsonApi.DependencyInjections;

namespace EShop.Tenancy.API.DependencyInjections.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            return services;    
        }

        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services
                .AddDbContextWithScoping<TenancyDbContext>(configuration);

            return services;
        }
    }
}
