using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.AddVariationDimension;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapAddVariationDimension(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/{id}/variation-dimensions", AddVariationDimensionAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> AddVariationDimensionAsync(
        [FromRoute] Guid id,
        [FromBody] AddVariationDimensionRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AddVariationDimensionCommand
        {
            ProductId = id,
            Name = request.Name,
            DisplayName = request.DisplayName,
            Values = request.Values,
            DisplayStyle = request.DisplayStyle
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created("", result);
    }
}
