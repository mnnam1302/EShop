using EShop.Inventory.API.Models;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Inventory.API.APIs;

public static class ReservationApis
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
            .MapPost("", CreateReservationV1Async)
            .RequirePermissionFilter(PermissionConstants.Inventory.ManageInventory);

        return routerBuilder;
    }

    private static async Task<IResult> CreateReservationV1Async(
        [FromBody] CreateReservationRequest request,
        [FromServices] IMediator mediator,
        [FromServices] IUserDetailsProvider userDetails,
        CancellationToken cancellationToken)
    {
        var user = userDetails.AuthenticatedUser;
        var command = new ReserveStocksCommand
        {
            OrderId = Guid.NewGuid(),
            Items = request.Items,
            TenantId = user.TenantId,
            ActionUserId = user.Id,
            ActionUserType = user.UserType
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created("", result);
    }
}
