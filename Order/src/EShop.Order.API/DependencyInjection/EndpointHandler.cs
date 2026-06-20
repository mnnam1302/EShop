using EShop.Order.API.APIs;

namespace EShop.Order.API.DependencyInjection;

public static class EndpointHandler
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOrderEndpoints();

        return endpoints;
    }
}

