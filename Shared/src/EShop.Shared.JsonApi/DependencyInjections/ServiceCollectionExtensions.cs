using EShop.Shared.DbResourceAccessControl;
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
        services.AddTransient<IMultiTenantIsolationStategy, PostgresMultiTenantConnectionInterceptor>();
        services.AddScoped<ITenantIsolationStrategy, PostgresRowLevelSecurityPolicyIsolation>();
        
        return services;
    }

    /// <summary>
    /// Replaces all registrations of service type <see cref="TService"/> in the <see cref="IServiceCollection"/>
    /// with new implementation type <see cref="TImplementation"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    /// <typeparam name="TService">The service type to be replaced.</typeparam>
    /// <typeparam name="TImplementation">The replacing service implementation.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection ReplaceAll<TService, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime)
    {
        services.RemoveAll<TService>();
        services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));

        return services;
    }
}