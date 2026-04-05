using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.UpdateVariant;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapUpdateVariant(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPut("/{id}/variants/{variantId}", UpdateVariantAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> UpdateVariantAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid variantId,
        [FromBody] UpdateVariantRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateVariantCommand
        {
            ProductId = id,
            VariantId = variantId,
            Name = request.Name,
            Sku = request.Sku
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
