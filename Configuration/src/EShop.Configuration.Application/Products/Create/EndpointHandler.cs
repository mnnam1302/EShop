using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Configuration.Application.Products.Create;

internal static class EndpointHandler
{
    public static RouteGroupBuilder MapCreateProduct(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/", CreateProductAsync)
            .RequirePermissionFilter(PermissionConstants.ManageProductsPermissionId)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return productEndpointBuilder;
    }

    private static async Task<IResult> CreateProductAsync(
        [FromBody] CreateProductRequest request)
    {
        await Task.CompletedTask;

        return TypedResults.Created();
    }
}
