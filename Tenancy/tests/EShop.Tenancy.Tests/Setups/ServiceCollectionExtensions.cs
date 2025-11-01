using Carter;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.EventBus.JsonConverters;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Tenancy.API;
using EShop.Tenancy.API.DependencyInjections.Extensions;
using EShop.Tenancy.Application.DependencyInjections;
using EShop.Tenancy.Application.Services;
using EShop.Tenancy.Infrastructure.DependencyInjections;
using EShop.Tenancy.Persistence;
using EShop.Tenancy.Persistence.DependencyInjections;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using EShop.Testing.JsonApiApplication.EventBus;
using EShop.Testing.JsonApiApplication.Providers;
using MassTransit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Tests.Setups;

public static class ServiceCollectionExtensions
{
    public static void AddTestShared(this IServiceCollection services, PostgreSqlTestDatabase testDatabase)
    {
        services.AddResiliencePolicy();
        services.AddMemoryCacheInfrastructure();
        services.AddPostgreSqlTestDbContext<TenancyDbContext>(testDatabase);
    }

    public static void AddTestBoostrapping(this IServiceCollection services)
    {
        services.AddTestTenancyAPI();
        services.AddTestTenancyApplication();
        services.AddTenancyPersistence();
        services.AddTestTenancyInfrastructure();
    }

    private static IServiceCollection AddTestTenancyAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddTransient<DbInitializer>();

        services.AddControllers().AddApplicationPart(Presentation.AssemblyReference.Assembly);
        services.AddCarter();

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

        services.AddTestAuthentication();

        return services;
    }

    private static void AddTestAuthentication(this IServiceCollection services)
    {
        services.AddRsaKeyServices();
        services.AddJwtTokenAuthentication();
        services.AddUserTokensProvider();
    }

    private static void AddRsaKeyServices(this IServiceCollection services)
    {
        services.AddMultiTenantKeyManager();
        services.AddRsaKeyCachingProvider();
    }

    private static void AddTestTenancyApplication(this IServiceCollection services)
    {
        services.AddMediatR();

        // permission validator
        services.AddScoped<IPermissionValidator, CurrentUserPermissionsValidator>();
        services.AddSingleton<IUserPermissionsProvider, TestUserPermissionProvider>();

        // feature validator
        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddScoped<ITenantFeaturesProvider, TestTenantFeatureProvider>();
        services.AddSingleton<ITenantFeaturesCachingService, TestTenantFeaturesCachingService>();
        services.AddScoped<IFeatureService, FeatureService>();
    }

    private static void AddTestTenancyInfrastructure(this IServiceCollection services)
    {
        services.AddEventBusGateway();
        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();
            cfg.AddConsumers(Infrastructure.AssemblyReference.Assembly);

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
    }
}
