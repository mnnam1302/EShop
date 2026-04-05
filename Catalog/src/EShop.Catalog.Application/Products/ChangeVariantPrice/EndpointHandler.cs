using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.ChangeVariantPrice;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapChangeVariantPrice(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPut("/{id}/variants/{variantId}/price", ChangeVariantPriceAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> ChangeVariantPriceAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid variantId,
        [FromBody] ChangeVariantPriceRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new ChangeVariantPriceCommand
        {
            ProductId = id,
            VariantId = variantId,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
