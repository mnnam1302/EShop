namespace EShop.Inventory.Domain.Abstractions;

/// <summary>
/// Fast-path Redis gate for stock availability.
/// Provides atomic check-and-reserve via Lua scripts.
/// Redis is the first line of defence; Postgres remains the authoritative source.
/// </summary>
public interface IRedisStockGateway
{
    /// <summary>
    /// Atomically checks and reserves the requested quantities.
    /// Phase 1: checks all items — Phase 2: reserves all or none.
    /// Returns <c>true</c> if all items are reserved, <c>false</c> if any item
    /// has insufficient stock (no partial reservation is made).
    /// </summary>
    Task<bool> TryReserveAsync(
        IReadOnlyList<StockReservationRequest> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically releases the previously reserved quantities back to available.
    /// </summary>
    Task ReleaseAsync(
        IReadOnlyList<StockReservationRequest> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the Redis available-stock counter for a variant directly from Postgres.
    /// Called during cold-start initialisation and the periodic sync job.
    /// </summary>
    Task SeedStockAsync(Guid variantId, int availableStock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if Redis stock data has been seeded (sentinel key exists).
    /// </summary>
    Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default);
}

/// <summary>Item request used in batch reserve / release operations.</summary>
public sealed class StockReservationRequest
{
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
}
