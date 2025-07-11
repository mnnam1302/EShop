using EShop.Configuration.Application.Products;

namespace EShop.Configuration.Application.Boostrapping;

public static class EndpointHandlers
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapProductEndpoints();

        return endpoints;
    }
}
