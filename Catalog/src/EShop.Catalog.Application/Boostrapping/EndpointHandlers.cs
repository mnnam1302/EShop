using EShop.Catalog.Application.Agencies;
using EShop.Catalog.Application.Categories;

namespace EShop.Catalog.Application.Boostrapping;

public static class EndpointHandlers
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints
            .MapAgencyEndpoints()
            .MapCategoryEndpoints();

        return endpoints;
    }
}
