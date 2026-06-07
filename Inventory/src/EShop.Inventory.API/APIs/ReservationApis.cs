using EShop.Inventory.API.Models;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Inventory.API.APIs;

internal static class ReservationApis
{
    private const string _baseUrl = "api/v{version:apiVersion}/reservations";

    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var endpointsV1 = routerBuilder
            .NewVersionedApi("Reservation")
            .MapGroup(_baseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Inventory.InventoryManagement);

        endpointsV1
            .MapPost("", CreateReservationAsyncV1)
            .RequirePermissionFilter(PermissionConstants.Inventory.ManageInventory);

        return routerBuilder;
    }

    private static async Task<IResult> CreateReservationAsyncV1(
        [FromBody] CreateReservationRequest request,
        [FromServices] IMediator mediator,
         CancellationToken cancellationToken)
    {
        var command = new MakeReservationsCommand
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
}
