using EShop.Catalog.Application.Products.Create;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Products;

public static class EndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/products";

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var productEndpointsV1 = routerBuilder
            .NewVersionedApi("Product")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Catalog.ProductBuilder_FeatureId);

        productEndpointsV1.MapCreateProduct();

        return routerBuilder;
    }
}
