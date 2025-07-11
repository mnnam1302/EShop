
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Configuration.Application.Products.Create;

internal static class EndpointHandler
{
    public static IEndpointRouteBuilder MapCreateProduct(this IEndpointRouteBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/", CreateProductAsync)
            .RequirePermissionFilter(PermissionConstants.ManageProductsPermissionId);

        return productEndpointBuilder;
    }

    private static async Task CreateProductAsync(HttpContext context)
    {
        throw new NotImplementedException();
    }
}
