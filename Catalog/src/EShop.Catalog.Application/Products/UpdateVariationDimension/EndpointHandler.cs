using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.UpdateVariationDimension;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapUpdateVariationDimension(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPut("/{id}/variation-dimensions/{name}", UpdateVariationDimensionAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> UpdateVariationDimensionAsync(
        [FromRoute] Guid id,
        [FromRoute] string name,
        [FromBody] UpdateVariationDimensionRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateVariationDimensionCommand
        {
            ProductId = id,
            Name = name,
            DisplayName = request.DisplayName,
            DisplayStyle = request.DisplayStyle
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
