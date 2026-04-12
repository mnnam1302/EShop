using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.Publish;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapPublishProduct(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/{id}/publish", PublishProductAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> PublishProductAsync(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new PublishProductCommand(id);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
