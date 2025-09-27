using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.DbResourceAccessControl.Interceptors;
using EShop.Shared.DbResourceAccessControl.Options;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EShop.Shared.JsonApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMultiTenantScoping(this IServiceCollection services)
    {
        services.AddOptions<DbResourceAccessControlOptions>()
                .BindConfiguration(DbResourceAccessControlOptions.SectionName);

        services.AddHttpContextAccessor();
        services.AddScoped<IUserDetailsProvider, HttpRequestUserDataProvider>();

        services.AddScoped<IMultiTenantIsolationStrategy, PostgresMultiTenantConnectionInterceptor>();
        services.AddScoped<ITenantIsolationStrategy, PostgresRowLevelSecurityPolicyIsolation>();

        return services;
    }

    public static IServiceCollection AddRingFencedScoping(this IServiceCollection services)
    {
        services.AddTransient<IRingFencingConnectionInterceptor, PostgresRingFencingConnectionInterceptor>();
        services.TryAddScoped<IRingFencingIsolationStrategy, PostgresRowLevelSecurityPolicyRingFencing>();

        return services;
    }

    public static IServiceCollection AddAuditableInterceptor(this IServiceCollection services)
    {
        services.TryAddScoped<AuditableInterceptor>();
        return services;
    }
}