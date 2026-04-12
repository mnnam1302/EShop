using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.UnpublishVariant;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapUnpublishVariant(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/{id}/variants/{variantId}/unpublish", UnpublishVariantAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> UnpublishVariantAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid variantId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UnpublishVariantCommand
        {
            ProductId = id,
            VariantId = variantId
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
