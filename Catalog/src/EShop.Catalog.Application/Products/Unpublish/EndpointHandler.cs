using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.Unpublish;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapUnpublishProduct(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/{id}/unpublish", UnpublishProductAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> UnpublishProductAsync(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UnpublishProductCommand(id);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
