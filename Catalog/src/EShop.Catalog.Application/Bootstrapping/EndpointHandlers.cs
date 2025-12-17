using EShop.Catalog.Application.Agencies;
using EShop.Catalog.Application.Categories;
using EShop.Catalog.Application.Products;

namespace EShop.Catalog.Application.Bootstrapping;

public static class EndpointHandlers
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints
            .MapAgencyEndpoints()
            .MapCategoryEndpoints()
            .MapProductEndpoints();

        return endpoints;
    }
}
