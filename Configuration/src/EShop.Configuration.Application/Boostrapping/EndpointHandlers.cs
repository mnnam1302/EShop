using EShop.Configuration.Application.Agencies;

namespace EShop.Configuration.Application.Boostrapping;

public static class EndpointHandlers
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapAgencyEndpoints();

        return endpoints;
    }
}
