using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.Delete;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapDeleteProduct(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/{id}/delete", DeleteProductAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> DeleteProductAsync(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
