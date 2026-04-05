using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.ChangeVariationDimensionValues;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapChangeVariationDimensionValues(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPut("/{id}/variation-dimensions/{name}/values", ChangeVariationDimensionValuesAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> ChangeVariationDimensionValuesAsync(
        [FromRoute] Guid id,
        [FromRoute] string name,
        [FromBody] ChangeVariationDimensionValuesRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ChangeVariationDimensionValuesCommand
        {
            ProductId = id,
            DimensionName = name,
            Values = request.Values
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
