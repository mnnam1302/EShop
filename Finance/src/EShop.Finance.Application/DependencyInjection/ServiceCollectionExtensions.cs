using EShop.Shared.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Finance.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        return services.AddMediator(AssemblyReference.Assembly);
    }
}
