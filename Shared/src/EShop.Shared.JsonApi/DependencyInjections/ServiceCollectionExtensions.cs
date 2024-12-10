using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserScoping(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserDetailsProvider, HttpRequestUserDataProvider>();
        return services;
    }

    public static IServiceCollection AddMultiTenantScoping(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserDetailsProvider, HttpRequestUserDataProvider>();
        services.AddTransient<IMultiTenantConnectionInterceptor, PostgresMultiTenantConnectionInterceptor>(); // why transient here?
        services.AddScoped<ITenantIsolationStrategy, PostgresRowLevelSecurityPolicyIsolation>();
        
        return services;
    }

    public static IServiceCollection PropagateStandardHeaders(this IServiceCollection services)
    {
        services.AddHeaderPropagation(options =>
        {
            options.Headers.Add("Authorization");
            options.Headers.Add("eshop-user-id");
        });

        return services;
    }
}