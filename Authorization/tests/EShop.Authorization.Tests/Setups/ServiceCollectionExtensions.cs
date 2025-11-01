using EShop.Authorization.API.Boostrapping;
using EShop.Authorization.Application.DependencyInjections;
using EShop.Authorization.Infrastructure;
using EShop.Authorization.Infrastructure.DependencyInjections;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.EventBus.JsonConverters;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using EShop.Testing.JsonApiApplication.EventBus;
using EShop.Testing.JsonApiApplication.Providers;
using MassTransit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Authorization.Tests.Setups;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestBoostrapping(this IServiceCollection services, PostgreSqlTestDatabase testDatabase, IConfiguration configuration)
    {
        services.AddTestAuthorizationAPI()
            .AddTestAuthorizationApplication()
            .AddTestAuthorizationPersistence(testDatabase)
            .AddTestAuthorizationInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddTestAuthorizationAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddResiliencePolicy();
        services.AddControllers().AddApplicationPart(API.AssemblyReference.Assembly);

        services.AddSwaggerGenNewtonsoftSupport()
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

        services.AddOptions<JwtOptions>().BindConfiguration(nameof(JwtOptions));
        services.AddTenantAuthenticationProvider();

        return services;
    }

    private static IServiceCollection AddTestAuthorizationApplication(this IServiceCollection services)
    {
        services.AddMediator(Authorization.Application.AssemblyReference.Assembly);
        services.AddApplicationServices();

        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();
        services.AddTransient<IUserPermissionsProvider, TestUserPermissionProvider>();

        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddScoped<ITenantFeaturesProvider, TestTenantFeatureProvider>();

        return services;
    }

    private static IServiceCollection AddTestAuthorizationPersistence(this IServiceCollection services, PostgreSqlTestDatabase testDatabase)
    {
        services.AddPostgreSqlTestDbContext<AuthorizationDbContext>(testDatabase)
            .AddPersistenceServices();

        return services;
    }

    private static IServiceCollection AddTestAuthorizationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEventBus()
            .AddMassTransitMemory();

        services.AddMemoryCacheInfrastructure();

        services.AddEmailServices(configuration);

        return services;
    }

    private static IServiceCollection AddMassTransitMemory(this IServiceCollection services)
    {
        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();
            cfg.AddConsumers(Authorization.Infrastructure.AssemblyReference.Assembly);

            cfg.UsingInMemory((context, bus) =>
            {
                bus.UseNewtonsoftJsonSerializer();
                bus.ConfigureNewtonsoftJsonSerializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });
                bus.ConfigureNewtonsoftJsonDeserializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });

                bus.ReceiveEndpoint("test_queue", configureEndpoint =>
                {
                    configureEndpoint.ConfigureConsumers(context);
                    configureEndpoint.Observer(new EventObserver<ITenantCreated>(context.GetRequiredService<IIntegrationEventsTracker>()));
                });
            });
        });

        return services;
    }
}
