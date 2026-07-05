using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Http;
using EShop.Finance.Application.Services.IntegrationProvider.Generic;
using EShop.Shared.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Finance.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        services.AddMediator(AssemblyReference.Assembly);
        services.AddAccountingIntegrationProviders();
        return services;
    }

    private static IServiceCollection AddAccountingIntegrationProviders(this IServiceCollection services)
    {
        services.AddScoped<IAccountingIntegrationProvider, NoneAccountingIntegrationProvider>();
        services.AddScoped<IAccountingIntegrationProvider, GenericHttpAccountingProvider>();
        services.AddScoped<IAccountingIntegrationProviderFactory, AccountingIntegrationProviderFactory>();

        services.AddScoped<IAuthenticationProvider, OAuthAuthenticationProvider>();
        services.AddScoped<IAuthenticationProvider, BasicAuthenticationProvider>();
        services.AddScoped<IAuthenticationProvider, NoAuthAuthenticationProvider>();
        services.AddScoped<IAuthenticationProviderResolver, AuthenticationProviderResolver>();

        services.AddScoped<IHttpIntegrationClient, HttpIntegrationClient>();

        return services;
    }
}
