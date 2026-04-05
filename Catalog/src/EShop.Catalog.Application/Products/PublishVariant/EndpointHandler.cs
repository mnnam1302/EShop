using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.PublishVariant;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapPublishVariant(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/{id}/variants/{variantId}/publish", PublishVariantAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> PublishVariantAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid variantId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new PublishVariantCommand
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
