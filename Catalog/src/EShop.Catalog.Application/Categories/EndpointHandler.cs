using EShop.Catalog.Application.Categories.Create;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Categories;

public static class EndpointHandler
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var categoryEndpointsV1 = routerBuilder
            .NewVersionedApi("Category")
            .MapGroup("api/v{version:apiVersion}/categories")
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Catalog.ProductBuilder_FeatureId);

        categoryEndpointsV1.MapCreateCategory();

        return routerBuilder;
    }
}
