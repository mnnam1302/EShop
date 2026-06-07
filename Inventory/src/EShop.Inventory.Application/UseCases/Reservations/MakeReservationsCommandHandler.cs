using EShop.Inventory.Application.Services;
using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Aggregates;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Inventory;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Application.UseCases.Reservations;

internal class MakeReservationsCommandHandler(
    IStockOrderCacheService stockOrderCacheService,
    IInventoryRepository inventoryRepository,
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    IUserDetailsProvider userDetailsProvider,
    IEventBus eventBus,
    ILogger<MakeReservationsCommandHandler> logger) : ICommandHandler<MakeReservationsCommand>
{
    // Concurrency & Idempotency - Currently, support one Inventory to simplify implementation
    public async Task<Result> HandleAsync(MakeReservationsCommand command, CancellationToken cancellationToken)
    {
        var authenticatedUser = userDetailsProvider.AuthenticatedUser;

        foreach (var item in command.Items)
        {
            // 1. Redis LUA: fast gate - same check as CAS
            int redisResult = await stockOrderCacheService.DecreaseStockCacheByLUA(item.VariantId, item.Quantity);
            if (redisResult == -1)
            {
                logger.LogInformation("StockDeduction: cache miss for variant='{VariantId}, warning up...'", item.VariantId);

                var warnedResult = await WarnStockToRedisAsync(item, cancellationToken);
                if (warnedResult.IsFailure)
                {
                    logger.LogInformation("StockDeduction: warning up is error since: {Message}", warnedResult.Error.Message);

                    await PublishFailedEventAsync(command.OrderId, warnedResult.Error, authenticatedUser, cancellationToken);
                    return warnedResult;
                }

                redisResult = await stockOrderCacheService.DecreaseStockCacheByLUA(item.VariantId, item.Quantity);
            }

            if (redisResult == 0)
            {
                logger.LogInformation("StockDeduction: stock available < quantity");

                var error = new Error("Inventory.StockDeduction", "Inventory is not enough quantity");
                await PublishFailedEventAsync(command.OrderId, error, authenticatedUser, cancellationToken);
                return Result.Failure(error);
            }

            // 2. Persist request to Reservation immediately
            var reservation = Reservation.Create(
                command.OrderId,
                item.VariantId,
                item.Quantity,
                expiresAt: DateTimeOffset.UtcNow.AddMinutes(30),
                authenticatedUser.TenantId);

            reservationRepository.Add(reservation);
            await inventoryRepository.DecreaseStockLevel1(item.VariantId, item.Quantity, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Publish Integration Event StockReserved | StockReservationFailed
            await eventBus.PublishAsync(new StockReserved
            {
                OrderId = command.OrderId,
                ReservationId = reservation.Id,
                TenantId = authenticatedUser.TenantId,
                ActionUserId = authenticatedUser.ActionUserId,
                ActionUserType = authenticatedUser.ActionUserType
            }, cancellationToken);
        }

        return Result.Success();
    }

    private async Task<Result> WarnStockToRedisAsync(OrderItem item, CancellationToken cancellationToken)
    {
        var inventoryDetails = await inventoryRepository.FindSingleAsync(
            i => i.VariantId == item.VariantId,
            cancellationToken: cancellationToken);

        if (inventoryDetails is null)
        {
            return Result.Failure(new Error("Inventory.StockDeduction", $"Variant's inventory '{item.VariantId}' is not found."));
        }

        await stockOrderCacheService.AddStockAvailable(item.VariantId, inventoryDetails.StockAvailable);
        return Result.Success();
    }

    private async Task PublishFailedEventAsync(Guid orderId, Error error, UserData authenticatedUser, CancellationToken cancellationToken)
    {
        var failedEvent = new StockReservationFailed
        {
            OrderId = orderId,
            FailureReason = error.Message,
            TenantId = authenticatedUser.TenantId,
            ActionUserId = authenticatedUser.ActionUserId,
            ActionUserType = authenticatedUser.ActionUserType
        };

        await eventBus.PublishAsync(failedEvent, cancellationToken);
        logger.LogInformation("Published StockReservationFailed for Order: {OrderId}. Reason: {Reason}", orderId, error.Message);
    }
}
