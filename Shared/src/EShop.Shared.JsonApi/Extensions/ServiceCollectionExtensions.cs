using EShop.Shared.JsonApi.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection GlobalExceptionHandlingMiddleware(this IServiceCollection services)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();
        return services;
    }
}