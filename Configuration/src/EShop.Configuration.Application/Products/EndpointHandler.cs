using EShop.Configuration.Application.Products.Create;

namespace EShop.Configuration.Application.Products;

internal static class EndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/products";

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var productEndpointsV1 = routerBuilder
            .NewVersionedApi("Products")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        productEndpointsV1
            .MapCreateProduct();

        return routerBuilder;
    }
}