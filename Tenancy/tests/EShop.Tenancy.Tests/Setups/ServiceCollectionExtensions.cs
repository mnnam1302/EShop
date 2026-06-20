using Carter;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Tenancy.API;
using EShop.Tenancy.API.Boostrapping;
using EShop.Tenancy.Application.DependencyInjections;
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
    public static IServiceCollection AddTestBoostrapping(this IServiceCollection services, PostgreSqlTestDatabase testDatabase)
    {
        services.AddTestTenancyAPI()
            .AddTestTenancyApplication()
            .AddTestTenancyPersistence(testDatabase)
            .AddTestTenancyInfrastructure();

        return services;
    }

    private static IServiceCollection AddTestTenancyAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddResiliencePolicy();
        services.AddGlobalExceptionMiddleware();
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

        services.AddOptions<JwtOptions>().BindConfiguration(nameof(JwtOptions));
        services.AddTenantAuthenticationProvider();

        return services;
    }

    private static IServiceCollection AddTestTenancyApplication(this IServiceCollection services)
    {
        services.AddMediator(Application.AssemblyReference.Assembly);

        services.AddScoped<IPermissionValidator, CurrentUserPermissionsValidator>();
        services.AddSingleton<IUserPermissionsProvider, TestUserPermissionProvider>();

        services.AddOwnerTenantFeaturesProvider();
        services.AddSingleton<ITenantFeaturesProvider, TestTenantFeatureProvider>();

        return services;
    }

    private static IServiceCollection AddTestTenancyPersistence(this IServiceCollection services, PostgreSqlTestDatabase testDatabase)
    {
        services.AddPostgreSqlTestDbContext<TenancyDbContext>(testDatabase);
        services.AddPersistenceServices();
        return services;
    }

    private static IServiceCollection AddTestTenancyInfrastructure(this IServiceCollection services)
    {
        services
            .AddMassTransitMemory()
            .AddEventBus();

        services.AddMemoryCacheInfrastructure();

        return services;
    }

    private static IServiceCollection AddMassTransitMemory(this IServiceCollection services)
    {
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

                bus.ConnectConsumeObserver(context.GetRequiredService<TestConsumeObserver>());

                bus.ReceiveEndpoint("test_queue", configureEndpoint =>
                {
                    configureEndpoint.ConfigureConsumers(context);
                });
            });
        });

        return services;
    }
}
