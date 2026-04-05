using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.AddVariant;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapAddVariant(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/{id}/variants", AddVariantAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return productEndpointBuilder;
    }

    private static async Task<IResult> AddVariantAsync(
        [FromRoute] Guid id,
        [FromBody] AddVariantRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new AddVariantCommand
        {
            ProductId = id,
            Name = request.Name,
            Sku = request.Sku,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            DimensionValues = request.DimensionValues
                .Select(dv => new VariantDimensionValueInput { Name = dv.Name, Value = dv.Value })
                .ToList()
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created("", result);
    }
}
