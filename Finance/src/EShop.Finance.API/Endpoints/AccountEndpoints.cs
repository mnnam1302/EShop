using EShop.Finance.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Finance.API.Endpoints;

public static class AccountEndpoints
{
    private const string BaseUrl = "api/v{version:apiVersion}/accounts";

    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var orderEndpointsV1 = routerBuilder
            .NewVersionedApi("Account")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        orderEndpointsV1.MapPost("{orderId:guid}", GetAccountByOrderIdAsync);

        return routerBuilder;
    }

    private static async Task<IResult> GetAccountByOrderIdAsync(
        [FromRoute] Guid orderId,
        IAccountRepository accounts,
        CancellationToken cancellationToken)
    {
        var account = await accounts.FindByOrderIdAsync(orderId, cancellationToken: cancellationToken);
        if (account is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new
        {
            account.Id,
            account.OrderId,
            account.Currency,
            account.PaymentFrequency,
            Status = account.Status.ToString(),
            account.TotalAmount,
            account.OutstandingAmount,
            Payments = account.Payments
                .OrderBy(i => i.Sequence)
                .Select(i => new
                {
                    i.Id,
                    i.Sequence,
                    i.Amount,
                    i.DueDate,
                    Status = i.Status.ToString(),
                    i.ExternalBookingReference,
                }),
        });
    }
}
