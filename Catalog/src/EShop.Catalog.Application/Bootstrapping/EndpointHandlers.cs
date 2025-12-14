using EShop.Catalog.Application.Agencies;
using EShop.Catalog.Application.Categories;

namespace EShop.Catalog.Application.Bootstrapping;

public static class EndpointHandlers
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints
            .MapAgencyEndpoints()
            .MapCategoryEndpoints();

        return endpoints;
    }
}
