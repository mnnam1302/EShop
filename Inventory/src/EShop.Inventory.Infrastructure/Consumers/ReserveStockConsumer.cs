//using EShop.Inventory.Domain.Abstractions;
//using EShop.Inventory.Domain.Entities;
//using EShop.Inventory.Domain.Enums;
//using EShop.Shared.Authentication.Abstractions;
//using EShop.Shared.Contracts.Abstractions.MessageBus;
//using EShop.Shared.Contracts.Services.Inventory;
//using EShop.Shared.Contracts.Services.Order.Saga;
//using MassTransit;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace EShop.Inventory.Infrastructure.Consumers;

///// <summary>
///// Handles <see cref="ReserveStockCommand"/> sent by the PlaceOrder saga.
/////
///// Flow:
/////   1. Redis fast-gate (Lua atomic check-and-reserve).
/////   2. Idempotency check on StockReservations table.
/////   3. Postgres: atomic stock adjustment + reservation row insert.
/////   4. Publish <see cref="StockReserved"/> or <see cref="StockReservationFailed"/>.
///// </summary>
//internal sealed class ReserveStockConsumer : IConsumer<ReserveStockCommand>
//{
//    private readonly InventoryDbContext _dbContext;
//    private readonly IRedisStockGateway _redisGateway;
//    private readonly IEventBus _eventBus;
//    private readonly IUserDetailsProvider _userDetailsProvider;
//    private readonly ILogger<ReserveStockConsumer> _logger;

//    private static readonly TimeSpan ReservationTtl = TimeSpan.FromMinutes(15);

//    public ReserveStockConsumer(
//        InventoryDbContext dbContext,
//        IRedisStockGateway redisGateway,
//        IEventBus eventBus,
//        IUserDetailsProvider userDetailsProvider,
//        ILogger<ReserveStockConsumer> logger)
//    {
//        _dbContext = dbContext;
//        _redisGateway = redisGateway;
//        _eventBus = eventBus;
//        _userDetailsProvider = userDetailsProvider;
//        _logger = logger;
//    }

//    public async Task Consume(ConsumeContext<ReserveStockCommand> context)
//    {
//        //var cmd = context.Message;

//        //using var _ = _userDetailsProvider.CreateSystemUserScope(
//        //    cmd.TenantId, cmd.ActionUserId, cmd.ActionUserType);

//        //// ── Idempotency check ──────────────────────────────────────────────────
//        //var existing = await _dbContext.StockReservations
//        //    .FirstOrDefaultAsync(r =>
//        //        r.IdempotencyKey == cmd.IdempotencyKey &&
//        //        r.Status == ReservationStatus.Active,
//        //        context.CancellationToken);

//        //if (existing is not null)
//        //{
//        //    _logger.LogWarning("Duplicate ReserveStockCommand for Order {OrderId} — replaying StockReserved.", cmd.OrderId);
//        //    await PublishStockReserved(cmd, existing.Id, context.CancellationToken);
//        //    return;
//        //}

//        //// ── Redis fast-gate ────────────────────────────────────────────────────
//        //var redisItems = cmd.Items.Select(i => new StockReservationRequest
//        //{
//        //    VariantId = i.VariantId,
//        //    Quantity = i.Quantity
//        //}).ToList();

//        //var redisOk = await _redisGateway.TryReserveAsync(redisItems, context.CancellationToken);

//        //if (!redisOk)
//        //{
//        //    _logger.LogInformation("Redis fast-gate rejected reservation for Order {OrderId}.", cmd.OrderId);
//        //    await PublishStockReservationFailed(cmd, "Insufficient stock (fast-gate).", context.CancellationToken);
//        //    return;
//        //}

//        //// ── Postgres authoritative check + persist ─────────────────────────────
//        //await using var tx = await _dbContext.Database.BeginTransactionAsync(context.CancellationToken);

//        //try
//        //{
//        //    foreach (var item in cmd.Items)
//        //    {
//        //        var inventory = await _dbContext.Inventories
//        //            .Where(i => i.VariantId == item.VariantId && i.TenantId == cmd.TenantId)
//        //            .FirstOrDefaultAsync(context.CancellationToken);

//        //        if (inventory is null || inventory.StockAvailable < item.Quantity)
//        //        {
//        //            await tx.RollbackAsync(context.CancellationToken);

//        //            // Compensate Redis — return the units we already reserved.
//        //            await _redisGateway.ReleaseAsync(redisItems, context.CancellationToken);

//        //            _logger.LogInformation("Postgres check failed for Order {OrderId}: insufficient stock for Variant {VariantId}.",
//        //                cmd.OrderId, item.VariantId);

//        //            await PublishStockReservationFailed(cmd, $"Insufficient stock for variant {item.VariantId}.", context.CancellationToken);
//        //            return;
//        //        }

//        //        inventory.StockAvailable -= item.Quantity;
//        //        inventory.ReservedStock += item.Quantity;

//        //        var reservation = StockReservation.Create(
//        //            orderId: cmd.OrderId,
//        //            variantId: item.VariantId,
//        //            quantity: item.Quantity,
//        //            idempotencyKey: cmd.IdempotencyKey,
//        //            expiresAt: DateTimeOffset.UtcNow.Add(ReservationTtl));

//        //        _dbContext.StockReservations.Add(reservation);
//        //    }

//        //    await _dbContext.SaveChangesAsync(context.CancellationToken);
//        //    await tx.CommitAsync(context.CancellationToken);

//        //    // Find first reservation to return as ReservationId (one per order for saga).
//        //    var firstReservation = await _dbContext.StockReservations
//        //        .Where(r => r.OrderId == cmd.OrderId && r.Status == ReservationStatus.Active)
//        //        .Select(r => r.Id)
//        //        .FirstOrDefaultAsync(context.CancellationToken);

//        //    await PublishStockReserved(cmd, firstReservation, context.CancellationToken);
//        //}
//        //catch (DbUpdateConcurrencyException ex)
//        //{
//        //    await tx.RollbackAsync(context.CancellationToken);
//        //    await _redisGateway.ReleaseAsync(redisItems, context.CancellationToken);

//        //    _logger.LogWarning(ex, "Concurrency conflict reserving stock for Order {OrderId} — will retry.", cmd.OrderId);
//        //    throw; // MassTransit retry policy applies
//        //}
//        //catch (Exception ex)
//        //{
//        //    await tx.RollbackAsync(context.CancellationToken);
//        //    await _redisGateway.ReleaseAsync(redisItems, context.CancellationToken);

//        //    _logger.LogError(ex, "Unexpected error reserving stock for Order {OrderId}.", cmd.OrderId);
//        //    throw;
//        //}
//    }

//    //private Task PublishStockReserved(ReserveStockCommand cmd, Guid reservationId, CancellationToken ct) =>
//    //    _eventBus.PublishAsync(new StockReserved
//    //    {
//    //        OrderId = cmd.OrderId,
//    //        ReservationId = reservationId,
//    //        ReservedAt = DateTimeOffset.UtcNow,
//    //        TenantId = cmd.TenantId,
//    //        ActionUserId = cmd.ActionUserId,
//    //        ActionUserType = cmd.ActionUserType
//    //    }, ct);

//    //private Task PublishStockReservationFailed(ReserveStockCommand cmd, string reason, CancellationToken ct) =>
//    //    _eventBus.PublishAsync(new StockReservationFailed
//    //    {
//    //        OrderId = cmd.OrderId,
//    //        Reason = reason,
//    //        FailedAt = DateTimeOffset.UtcNow,
//    //        TenantId = cmd.TenantId,
//    //        ActionUserId = cmd.ActionUserId,
//    //        ActionUserType = cmd.ActionUserType
//    //    }, ct);
//}
