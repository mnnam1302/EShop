using EShop.Shared.DbResourceAccessControl.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EShop.Shared.DbResourceAccessControl.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTenantIsolationScoping(this IServiceCollection services)
    {
        services.AddTransient<IMultiTenantIsolationStrategy, PostgresMultiTenantConnectionInterceptor>();
        services.AddScoped<ITenantIsolationStrategy, PostgresRowLevelSecurityPolicyIsolation>();
    }

    public static void AddRingFencedScoping(this IServiceCollection services)
    {
        services.AddTransient<IRingFencingConnectionInterceptor, PostgresRingFencingConnectionInterceptor>();
        services.TryAddScoped<IRingFencingIsolationStrategy, PostgresRowLevelSecurityPolicyRingFencing>();
    }

    public static void AddDomainEventsDispatcherInterceptor(this IServiceCollection services)
    {
        services.AddTransient<IDispatchDomainEventsInterceptor, DispatchDomainEventsInterceptor>();
    }
}
