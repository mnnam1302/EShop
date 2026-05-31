using EShop.Order.API.Models;
using EShop.Order.Domain.Commands;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Order.API.APIs;

public static class OrderApis
{
    private const string _baseUrl = "api/v{version:apiVersion}/orders";

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var orderEndpointsV1 = routerBuilder
            .NewVersionedApi("Order")
            .MapGroup(_baseUrl)
            .HasApiVersion(1);

        orderEndpointsV1.MapPost("", PlaceOrderV1Async);

        return routerBuilder;
    }

    private static async Task<IResult> PlaceOrderV1Async(
        [FromBody] PlaceOrderRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand
        {
            BuyerId = request.BuyerId,
            OrderItems = request.OrderItems.Select(oi => new OrderItemData
            {
                VariantId = oi.VariantId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Discount = oi.Discount
            }).ToList()
        };

        var result = await mediator.SendAsync<PlaceOrderCommand, Guid>(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        var orderId = result.Value;
        return Results.Accepted($"/api/v1/orders/{orderId}", new { orderId });
    }
}
