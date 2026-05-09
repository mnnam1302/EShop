using EShop.Shared.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Inventory.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services)
    {
        services.AddMediator(AssemblyReference.Assembly);
        services.AddValidatorsFromAssembly(Shared.Contracts.AssemblyReference.Assembly);

        return services;
    }
}
