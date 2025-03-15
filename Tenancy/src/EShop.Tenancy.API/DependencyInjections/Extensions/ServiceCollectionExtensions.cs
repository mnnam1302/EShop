using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Tenancy.Application.DependencyInjections.Extensions;
using EShop.Tenancy.Infrastructure.DependencyInjections.Extensions;
using EShop.Tenancy.Persistence;
using EShop.Tenancy.Persistence.DependencyInjections.Extensions;
using EShop.Tenancy.Presentation.DependencyInjections.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Tenancy.API.DependencyInjections.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services
                .AddDbContextWithScoping<TenancyDbContext>(configuration);

            services.AddResiliencePolicy();

            return services;
        }

        public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddUserPermissionsProvider(configuration);

            services.AddTenancyPresentation(); // Must before API project, because contain DI Carter
            services.AddTenancyAPI();
            services.AddTenancyPersistence();
            services.AddTenancyInfrastructure(configuration);
            services.AddTenancyApplication();

            return services;
        }

        private static void AddTenancyAPI(this IServiceCollection services)
        {
            services.AddCors();
            services.AddSingleton<ExceptionHandlingMiddleware>();

            services
                .AddSwaggerGenNewtonsoftSupport()
                .AddFluentValidationRulesToSwagger()
                .AddEndpointsApiExplorer()
                .AddSwaggerAPI();

            services
                .AddApiVersioning(options => options.ReportApiVersions = true)
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });
        }
    }
}