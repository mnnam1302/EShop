using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.Application.Abstractions;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Infrastructure.Authentication;
using EShop.Identity.Infrastructure.HashServices;
using EShop.Identity.Persistence;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.EventBus.JsonConverters;
using EShop.Shared.EventBus.Services;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using EShop.Testing.JsonApiApplication.EventBus;
using EShop.Testing.JsonApiApplication.Providers;
using MassTransit;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Tests.Setups;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestShared(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        PostgreSqlTestDatabase testDatabase)
    {
        services.AddResiliencePolicy();

        services.AddPostgreSqlTestDbContext<UsersDbContext>(testDatabase);
        services.AddMultiTenantScoping();

        services.AddMemoryInfrastructure();
        return services;
    }

    public static IServiceCollection AddTestBoostrapping(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddTestIdentityApi()
            .AddIdentityApplication()
            .AddIdentityPersistence()
            .AddTestIdentityInfrastructure();

        services.AddScoped<IPermissionValidator, CurrentUserPermissionsValidator>();
        services.AddSingleton<IUserPermissionsProvider, TestUserPermissionProvider>();

        services.AddScoped<IUserTokenCachingService, UserTokenRedisCachingService>();
        services.AddSingleton<
            IRedisCachingProvider<EShop.Shared.Contracts.Services.Identity.Auth.Response.AuthenticatedResponse>, TestUserTokenProvider>();

        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddSingleton<ITenantFeaturesProvider, TestTenantFeatureProvider>();

        return services;
    }

    private static IServiceCollection AddTestIdentityApi(this IServiceCollection services)
    {
        services.AddCors();
        //services.AddTransient<ExceptionHandlingMiddleware>();

        services.AddControllers()
            .AddApplicationPart(Identity.Presentation.AssemblyReference.Assembly);

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

        return services;
    }

    private static IServiceCollection AddTestIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ITokenService, TokenService>();
        services.AddScoped<IEventBusGateway, EventBusGateway>();

        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();
            cfg.AddConsumers(Identity.Infrastructure.AssemblyReference.Assembly);

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