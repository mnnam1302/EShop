using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.DbResourceAccessControl.Interceptors;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        services.AddTransient<IMultiTenantIsolationStrategy, PostgresMultiTenantConnectionInterceptor>();
        services.AddScoped<ITenantIsolationStrategy, PostgresRowLevelSecurityPolicyIsolation>();

        return services;
    }

    public static IServiceCollection AddMultiTenantScopingV2(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IUserDetailsProvider, HttpRequestUserDataProvider>();
        services.AddTransient<IMultiTenantIsolationStrategy, PostgresMultiTenantConnectionInterceptor>();
        services.AddTransient<ITenantIsolationStrategy, PostgresRowLevelSecurityPolicyIsolation>();

        return services;
    }

    public static IServiceCollection AddAuditableInterceptor(this IServiceCollection services)
    {
        services.TryAddScoped<AuditableInterceptor>();
        return services;
    }
}