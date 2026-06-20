using EShop.Inventory.Application.Services;
using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Inventory;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Application.UseCases.Inventories;

internal sealed class ReserveStocksCommandHandler(
    IStockOrderCacheService stockOrderCacheService,
    IInventoryRepository inventoryRepository,
    IUnitOfWork unitOfWork,
    IEventBus eventBus,
    ILogger<ReserveStocksCommandHandler> logger) : ICommandHandler<ReserveStocksCommand>
{
    private const int MaxConcurrencyRetries = 3;

    public async Task<Result> HandleAsync(ReserveStocksCommand command, CancellationToken cancellationToken)
    {
        var lockedCacheItems = new List<(Guid VariantId, int Quantity)>();

        // =========================================================================
        // PHASE 1: DISTRIBUTED FAST-GATE GUARD (REDIS LUA VALIDATION & DEDUCTION)
        // =========================================================================
        foreach (var item in command.Items)
        {
            int redisResult = await stockOrderCacheService.DecreaseStockCacheByLUA(item.VariantId, item.Quantity);
            if (redisResult == -1)
            {
                logger.LogInformation("StockReservation: cache miss for variant '{VariantId}', warming up...", item.VariantId);

                var warnedResult = await WarnStockToRedisAsync(item, cancellationToken);
                if (warnedResult.IsFailure)
                {
                    await PublishFailedEventAsync(command, warnedResult.Error, cancellationToken);
                    await RollbackCacheOnlyAsync(lockedCacheItems);
                    return warnedResult;
                }

                redisResult = await stockOrderCacheService.DecreaseStockCacheByLUA(item.VariantId, item.Quantity);
            }

            if (redisResult == 0)
            {
                logger.LogWarning("StockReservation: insufficient stock for variant '{VariantId}'", item.VariantId);

                var error = new Error("Inventory.StockReservation", $"Product '{item.VariantId}' is out of stock.");
                await PublishFailedEventAsync(command, error, cancellationToken);
                await RollbackCacheOnlyAsync(lockedCacheItems);
                return Result.Failure(error);
            }

            if (redisResult == 1)
            {
                lockedCacheItems.Add((item.VariantId, item.Quantity));
            }
        }

        // =========================================================================
        // PHASE 2: PERSISTENCE WITH OPTIMISTIC LOCKING (RETRYABLE)
        // =========================================================================
        var retryCount = 0;
        while (retryCount < MaxConcurrencyRetries)
        {
            try
            {
                foreach (var item in command.Items)
                {
                    await inventoryRepository.DecreaseStockLevel1(item.VariantId, item.Quantity, cancellationToken);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                break;
            }
            catch (DbUpdateConcurrencyException)
            {
                retryCount++;
                if (retryCount >= MaxConcurrencyRetries)
                {
                    logger.LogError("StockReservation: Concurrency conflict after {Retries} retries for Order {OrderId}", retryCount, command.OrderId);
                    var error = new Error("Inventory.StockReservation.Conflict", "Failed to reserve stock due to concurrent modifications.");
                    await PublishFailedEventAsync(command, error, cancellationToken);
                    await RollbackCacheOnlyAsync(lockedCacheItems);
                    return Result.Failure(error);
                }

                logger.LogWarning("StockReservation: Concurrency conflict (attempt {Attempt}/{Max}), retrying for Order {OrderId}", retryCount, MaxConcurrencyRetries, command.OrderId);
                await Task.Delay(100 * retryCount, cancellationToken);
            }
        }

        // =========================================================================
        // PHASE 3: INTEGRATION EVENT DISPATCHING
        // =========================================================================
        await eventBus.PublishAsync(new StockReserved
        {
            OrderId = command.OrderId,
            TenantId = command.TenantId,
            ActionUserId = command.ActionUserId,
            ActionUserType = command.ActionUserType
        }, cancellationToken);

        logger.LogInformation("StockReservation: Successfully reserved stock for Order {OrderId}", command.OrderId);
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

    private async Task PublishFailedEventAsync(ReserveStocksCommand command, Error error, CancellationToken cancellationToken)
    {
        var failedEvent = new StockReservationFailed
        {
            OrderId = command.OrderId,
            FailureReason = error.Message,
            TenantId = command.TenantId,
            ActionUserId = command.ActionUserId,
            ActionUserType = command.ActionUserType
        };

        await eventBus.PublishAsync(failedEvent, cancellationToken);
        logger.LogInformation("Published StockReservationFailed for Order: {OrderId}. Reason: {Reason}", command.OrderId, error.Message);
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
