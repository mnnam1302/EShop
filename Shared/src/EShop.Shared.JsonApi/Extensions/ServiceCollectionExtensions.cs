using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.Managers;
using EShop.Shared.DbResourceAccessControl.Extensions;
using EShop.Shared.DbResourceAccessControl.Options;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMultiTenantScoping(this IServiceCollection services)
    {
        services.AddOptions<DbResourceAccessControlOptions>()
            .BindConfiguration(DbResourceAccessControlOptions.SectionName);

        services.AddHttpContextAccessor();
        services.AddScoped<IUserDetailsProvider, HttpRequestUserDataProvider>();

        services.AddTenantIsolationScoping();

        return services;
    }
}