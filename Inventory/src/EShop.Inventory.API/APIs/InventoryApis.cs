using EShop.Inventory.API.Models;
using EShop.Inventory.Application.UseCases.Inventory;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Inventory.API.APIs;

public static class InventoryApis
{
    private const string _baseUrl = "api/v{version:apiVersion}/inventories";

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var inventoryEndpointsV1 = routerBuilder
            .NewVersionedApi("Inventory")
            .MapGroup(_baseUrl)
            .HasApiVersion(1);
        //.RequireFeatureFilter(FeatureConstants.Inventory.InventoryManagement);

        inventoryEndpointsV1.MapPost("", CreateInventoriesV1Async);

        return routerBuilder;
    }

    private static async Task<IResult> CreateInventoriesV1Async(
        [FromBody] CreateInventoryRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateInventoryCommand
        {
            ProductId = request.ProductId,
            VariantId = request.VariantId,
            Sku = request.Sku,
            StockAvailable = request.StockAvailable,
            MinimumStock = request.MinimumStock
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created("", result);
    }
}
