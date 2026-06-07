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

internal sealed class MakeReservationsCommandHandler(
    IStockOrderCacheService stockOrderCacheService,
    IInventoryRepository inventoryRepository,
    IUnitOfWork unitOfWork,
    IUserDetailsProvider userDetailsProvider,
    IEventBus eventBus,
    ILogger<MakeReservationsCommandHandler> logger) : ICommandHandler<MakeReservationsCommand>
{
    public async Task<Result> HandleAsync(MakeReservationsCommand command, CancellationToken cancellationToken)
    {
        var authenticatedUser = userDetailsProvider.AuthenticatedUser;
        var lockedCacheItems = new List<(Guid VariantId, int Quantity)>();

        // =========================================================================
        // PHASE 1: DISTRIBUTED FAST-GATE GUARD (REDIS LUA VALIDATION & DEDUCTION)
        // =========================================================================
        foreach (var item in command.Items)
        {
            // Case 1: Cache Miss
            int redisResult = await stockOrderCacheService.DecreaseStockCacheByLUA(item.VariantId, item.Quantity);
            if (redisResult == -1)
            {
                logger.LogInformation("StockDeduction: cache miss for variant '{VariantId}', warming up...", item.VariantId);

                var warnedResult = await WarnStockToRedisAsync(item, cancellationToken);
                if (warnedResult.IsFailure)
                {
                    await PublishFailedEventAsync(command.OrderId, warnedResult.Error, authenticatedUser, cancellationToken);
                    await RollbackCacheOnlyAsync(lockedCacheItems);
                    return warnedResult;
                }

                redisResult = await stockOrderCacheService.DecreaseStockCacheByLUA(item.VariantId, item.Quantity);
            }

            // Case 2: Out of Stock
            if (redisResult == 0)
            {
                logger.LogWarning("StockDeduction: stock available < quantity for variant '{VariantId}'", item.VariantId);

                var error = new Error("Inventory.StockDeduction", $"Product '{item.VariantId}' is out of stock.");
                await PublishFailedEventAsync(command.OrderId, error, authenticatedUser, cancellationToken);

                // Compensate and release previously locked item stock back to the public pool
                await RollbackCacheOnlyAsync(lockedCacheItems);
                return Result.Failure(error);
            }

            // Case 3: Success
            if (redisResult == 1)
            {
                lockedCacheItems.Add((item.VariantId, item.Quantity));
            }
        }

        // =========================================================================
        // PHASE 2: PERSISTENCE STATE & RELATIONAL ATOMICITY (POSTGRES DB GATES)
        // =========================================================================
        foreach (var item in command.Items)
        {
            await inventoryRepository.DecreaseStockLevel1(item.VariantId, item.Quantity, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // =========================================================================
        // PHASE 3: INTEGRATION EVENT DISPATCHING (HAPPY PATH OUTBOX/PUBLISH)
        // =========================================================================
        await eventBus.PublishAsync(new StockReserved
        {
            OrderId = command.OrderId,
            TenantId = authenticatedUser.TenantId,
            ActionUserId = authenticatedUser.ActionUserId,
            ActionUserType = authenticatedUser.ActionUserType
        }, cancellationToken);

        logger.LogInformation("StockDeduction: Successfully reserved stock for Order {OrderId}", command.OrderId);
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

    private async Task RollbackCacheOnlyAsync(List<(Guid VariantId, int Quantity)> itemsToRollback)
    {
        if (itemsToRollback.Count == 0)
        {
            return;
        }

        logger.LogInformation("RollbackCache: Initiating compensating logic on Redis for {Count} items.", itemsToRollback.Count);

        foreach (var item in itemsToRollback)
        {
            await stockOrderCacheService.IncreaseStockCache(item.VariantId, item.Quantity);
        }
    }
}
