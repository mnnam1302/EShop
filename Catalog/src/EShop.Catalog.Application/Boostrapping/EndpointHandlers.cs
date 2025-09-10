using EShop.Catalog.Application.Agencies;

namespace EShop.Catalog.Application.Boostrapping;

public static class EndpointHandlers
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapAgencyEndpoints();

        return endpoints;
    }
}
