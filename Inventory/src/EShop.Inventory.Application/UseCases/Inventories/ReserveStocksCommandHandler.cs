using EntityFramework.Exceptions.Common;
using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Aggregates;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Inventory;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EShop.Inventory.Application.UseCases.Inventories;

internal sealed class ReserveStocksCommandHandler(
    IRedisStockGateway redisStockGateway,
    IInventoryRepository inventoryRepository,
    IReservationRepository reservationRepository,
    IOutboxWriter outboxWriter,
    IUnitOfWork unitOfWork,
    IEventBus eventBus,
    ILogger<ReserveStocksCommandHandler> logger) : ICommandHandler<ReserveStocksCommand>
{
    private const int MaxDeadlockRetries = 3;
    private const int LinearBackoffDelayMs = 50;
    private const int ReservationExpiryMinutes = 15;

    private const string ErrorDeadlockExceeded = "Inventory.StockReservation.DeadlockExhausted";
    private const string ErrorInventoryNotFound = "Inventory.StockReservation.SkuNotFound";
    private const string ErrorInsufficientStock = "Inventory.StockReservation.InsufficientStock";

    public async Task<Result> HandleAsync(ReserveStocksCommand command, CancellationToken cancellationToken)
    {
        // DEADLOCK: Sort items by ID to enforce identical lock order across all database transactions.
        var itemsOrderedByLockSequence = command.Items.OrderBy(i => i.VariantId).ToList();

        var redisReservationRequests = itemsOrderedByLockSequence.Select(i => new StockReservationRequest
        {
            VariantId = i.VariantId,
            Quantity = i.Quantity
        }).ToList();

        // PHASE 1: REDIS FAST-GATE (High-performance in-memory check, all-or-nothing; runs before opening a transaction)
        var redisGateResult = await ValidateAndReserveOnRedisGateAsync(
            redisReservationRequests,
            command,
            cancellationToken);

        if (redisGateResult.IsFailure)
        {
            return redisGateResult;
        }

        // PHASE 2: DATABASE TRANSACTION (ACID Persistence with Deadlock Retry)
        for (var attempt = 0; attempt <= MaxDeadlockRetries; attempt++)
        {
            try
            {
                return await DeductStocksAsync(command, itemsOrderedByLockSequence, redisReservationRequests, cancellationToken);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == "40P01" &&
                attempt < MaxDeadlockRetries)
            {
                logger.LogWarning("DB Deadlock detected. Retrying {Attempt}/{Max} for Order {OrderId}.", attempt + 1, MaxDeadlockRetries, command.OrderId);

                // Allow database to clear blocks before retrying
                await Task.Delay(LinearBackoffDelayMs * (attempt + 1), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Unexpected error: Compensate and release Redis stock to prevent "ghost" lockouts
                await redisStockGateway.ReleaseAsync(redisReservationRequests, cancellationToken);
                logger.LogError(ex, "Fatal error executing transaction for Order {OrderId}.", command.OrderId);
                throw;
            }
        }

        // Retries exhausted
        await redisStockGateway.ReleaseAsync(redisReservationRequests, cancellationToken);

        var deadlockError = new Error(ErrorDeadlockExceeded, "Resource deadlock. Please try placing your order again.");
        await PublishFailedEventAsync(command, deadlockError, cancellationToken);

        return Result.Failure(deadlockError);
    }

    private async Task<Result> ValidateAndReserveOnRedisGateAsync(
        List<StockReservationRequest> redisItems,
        ReserveStocksCommand command,
        CancellationToken cancellationToken)
    {
        foreach (var item in redisItems)
        {
            var isReservedOnRedis = await redisStockGateway.TryReserveAsync([item], cancellationToken);

            // False means either out of stock OR cache miss (cold cache)
            if (!isReservedOnRedis)
            {
                /* * ─────────────────────────────────────────────────────────────────────────────
                 * ⚠️ TECHNICAL DEBT / FUTURE ENHANCEMENT WARNING: CACHE STAMPEDE (THUNDERING HERD)
                 * ─────────────────────────────────────────────────────────────────────────────
                 * RISK: If a hot item experiences a cache miss, 100+ concurrent requests will
                 * bypass Redis and smash the database simultaneously via FindSingleAsync().
                 * TODO / TO ENHANCE LATER:
                 * Implement an asynchronous distributed lock (e.g., Redlock) or local SemaphoreSlim
                 * wrapped around this query block. Only ONE request should fetch from DB and seed
                 * Redis; the remaining 99 requests must wait, then read the newly seeded Redis cache.
                 * ─────────────────────────────────────────────────────────────────────────────
                 */
                var databaseInventory = await inventoryRepository.FindSingleAsync(
                    i => i.VariantId == item.VariantId, cancellationToken: cancellationToken);

                if (databaseInventory is null)
                {
                    var notFoundError = new Error(ErrorInventoryNotFound, $"SKU {item.VariantId} does not exist.");

                    await PublishFailedEventAsync(command, notFoundError, cancellationToken);
                    return Result.Failure(notFoundError);
                }

                // Self-Seeding Mechanism: Populate Redis with DB state, then try reserving again
                await redisStockGateway.SeedStockAsync(item.VariantId, databaseInventory.StockAvailable, cancellationToken);
                isReservedOnRedis = await redisStockGateway.TryReserveAsync([item], cancellationToken);
            }

            if (!isReservedOnRedis)
            {
                var outOfStockError = new Error(ErrorInsufficientStock, $"SKU {item.VariantId} has insufficient stock.");
                await PublishFailedEventAsync(command, outOfStockError, cancellationToken);
                return Result.Failure(outOfStockError);
            }
        }

        return Result.Success();
    }

    private async Task<Result> DeductStocksAsync(
        ReserveStocksCommand command,
        List<OrderItem> sortedItems,
        List<StockReservationRequest> redisItems,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Optimistic Locking (Compare-And-Swap) deduction
            foreach (var item in sortedItems)
            {
                var rowsAffected = await inventoryRepository.DeductStockCasAsync(item.VariantId, command.TenantId, item.Quantity, cancellationToken);

                if (rowsAffected == 0) // Stock changed by another thread between Phase 1 and Phase 2
                {
                    // All-or-nothing: one short item → roll back entire transaction.
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    await redisStockGateway.ReleaseAsync(redisItems, cancellationToken);

                    var insufficientError = new Error(ErrorInsufficientStock, $"SKU {item.VariantId} stock mismatch during database write.");
                    await PublishFailedEventAsync(command, insufficientError, cancellationToken);
                    return Result.Failure(insufficientError);
                }
            }

            // 2. Track reservation inside DB (Pending state with 15m expiration)
            var expirationTime = DateTimeOffset.UtcNow.AddMinutes(ReservationExpiryMinutes);
            var reservation = Reservation.Create(command.OrderId, expirationTime, command.TenantId);

            foreach (var item in sortedItems)
            {
                reservation.AddItem(item.VariantId, item.Quantity);
            }

            reservationRepository.Add(reservation);

            // 3. Transactional Outbox Pattern: Event commits or rolls back atomically with the stock deduction
            outboxWriter.Enqueue(new StockReserved
            {
                OrderId = command.OrderId,
                ReservationId = reservation.Id,
                TenantId = command.TenantId,
                ActionUserId = command.ActionUserId,
                ActionUserType = command.ActionUserType
            });

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Stock committed successfully for Order {OrderId}.", command.OrderId);
            return Result.Success();
        }
        catch (DbUpdateException ex) when (ex is UniqueConstraintException)
        {
            // IDEMPOTENCY GUARD: UNIQUE(tenant_id, order_id) constraint violated.
            // Means network dropped on previous success and Client resent the command. Safely ACK & ignore.
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            await redisStockGateway.ReleaseAsync(redisItems, cancellationToken);

            logger.LogInformation("Duplicate request for Order {OrderId} detected. Safely ignored.", command.OrderId);
            return Result.Success();
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task PublishFailedEventAsync(ReserveStocksCommand command, Error error, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new StockReservationFailed
        {
            OrderId = command.OrderId,
            FailureReason = error.Message,
            TenantId = command.TenantId,
            ActionUserId = command.ActionUserId,
            ActionUserType = command.ActionUserType
        }, cancellationToken);

        logger.LogInformation("Published StockReservationFailed for Order {OrderId}. Reason: {Reason}", command.OrderId, error.Message);
    }
}
