using EShop.Inventory.API.Models;
using EShop.Inventory.Application.UseCases.Inventories;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.Contracts.Abstractions.Pagination;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Inventory.API.APIs;

public static class InventoryApis
{
    private const string _baseUrl = "api/v{version:apiVersion}/inventories";

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var endpointsV1 = routerBuilder
            .NewVersionedApi("Inventory")
            .MapGroup(_baseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Inventory.InventoryManagement);

        endpointsV1
            .MapPost("", CreateInventoriesV1Async)
            .RequirePermissionFilter(PermissionConstants.Inventory.ManageInventory);

        endpointsV1
            .MapGet("", GetInventoriesByProductIdV1Async)
            .RequireOneOfPermissionsFilter(PermissionConstants.Inventory.ViewInventory, PermissionConstants.Inventory.ManageInventory);

        endpointsV1
            .MapPatch("", WarnUpStockInventoryV1Async)
            .RequirePermissionFilter(PermissionConstants.Inventory.ManageInventory);

        endpointsV1
            .MapPut("", ReserveStocksAsyncV1)
            .RequirePermissionFilter(PermissionConstants.Inventory.ManageInventory);

        return routerBuilder;
    }

    private static async Task<IResult> ReserveStocksAsyncV1(
        [FromBody] CreateReservationRequest request,
        [FromServices] IMediator mediator,
         CancellationToken cancellationToken)
    {
        var command = new ReserveStocksCommand
        {
            OrderId = request.OrderId ?? Guid.NewGuid(),
            Items = request.Items
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created("", result);
    }

    private static async Task<IResult> GetInventoriesByProductIdV1Async(
        [FromQuery] Guid productId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var paginationRequest = PaginationRequest.Create(pageIndex, pageSize);

        var query = new GetInventoriesByProductQuery(productId, paginationRequest.PageIndex, paginationRequest.PageSize);

        var result = await mediator.QueryAsync<GetInventoriesByProductQuery, PaginationResult<InventoryDto>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Ok(result);
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

    private static async Task<IResult> WarnUpStockInventoryV1Async(
        [FromBody] WarnUpStockAvailableRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new WarnUpStockAvailableCommand
        {
            VariantId = request.VariantId
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Accepted();
    }
}
