# Inventory Microservice

> **Domain-Driven Design** | **CQRS** | **Event Sourcing** | **Clean Architecture**

The **Inventory** service is the single source of truth for stock levels across the entire eShop platform. It tracks per-SKU availability, manages stock reservations for the order lifecycle, and emits alerts when stock falls below configured thresholds.

---

## Table of Contents

1. [Bounded Context](#1-bounded-context)
2. [Ubiquitous Language](#2-ubiquitous-language)
3. [Domain Model](#3-domain-model)
   - [Aggregate Roots](#31-aggregate-roots)
   - [Domain Events](#32-domain-events)
   - [Value Objects](#33-value-objects)
   - [Specifications / Invariants](#34-specifications--invariants)
4. [Use Cases](#4-use-cases)
   - [Commands](#41-commands)
   - [Queries](#42-queries)
5. [Integration Events](#5-integration-events)
   - [Published](#51-published-by-inventory-service)
   - [Consumed](#52-consumed-by-inventory-service)
6. [Stock Lifecycle](#6-stock-lifecycle)
7. [Data Flows](#7-data-flows)
   - [Variant Created → Inventory Initialized](#71-variant-created--inventory-initialized)
   - [Order Placed → Stock Reserved](#72-order-placed--stock-reserved)
   - [Order Cancelled → Stock Released](#73-order-cancelled--stock-released)
   - [Order Fulfilled → Stock Deducted](#74-order-fulfilled--stock-deducted)
   - [Manual Stock Receipt](#75-manual-stock-receipt)
8. [API Endpoints](#8-api-endpoints)
9. [Architecture & Project Structure](#9-architecture--project-structure)
10. [Infrastructure](#10-infrastructure)
11. [Configuration](#11-configuration)
12. [Testing Strategy](#12-testing-strategy)

---

## 1. Bounded Context

The **Inventory bounded context** owns all concerns related to **physical stock quantity**. It explicitly does **not** own:

| Concern | Owned By |
|---|---|
| Product/variant catalogue data | Catalog service |
| Order orchestration and payment | Order service |
| Pricing and discounts | Catalog service |
| Shipment / fulfilment | (future Fulfilment service) |

**Context Map relationships:**

```
┌─────────────────┐   VariantCreated   ┌──────────────────────┐
│  Catalog Service│──────────────────► │  Inventory Service   │
│  (Upstream)     │   ProductDeleted   │  (Downstream)        │
└─────────────────┘                    │                      │
                                       │  StockReserved ──────►│  Order Service
┌─────────────────┐   OrderCreated     │  StockReservation    │  (Downstream)
│  Order Service  │──────────────────► │  Failed              │
│  (Upstream)     │   OrderCancelled   │  LowStockAlert ─────►│  (Notification)
└─────────────────┘   OrderCompleted   └──────────────────────┘
```

---

## 2. Ubiquitous Language

| Term | Definition |
|---|---|
| **SKU** (Stock Keeping Unit) | A unique code that identifies a specific product variant (size, colour, etc.). The primary unit of inventory tracking. |
| **InventoryItem** | The aggregate that tracks stock for a single SKU within one tenant. |
| **Available Stock** | Quantity that customers may currently purchase (`StockAvailable`). |
| **Reserved Stock** | Quantity held against pending orders — not yet permanently removed (`ReservedStock`). |
| **Physical Stock** | Total items on hand: `StockAvailable + ReservedStock`. |
| **Minimum Stock** | Low-watermark threshold; when `StockAvailable` falls at or below this value a `LowStockDetected` domain event is raised. |
| **Stock Receipt** | Inbound goods recorded against an inventory item, increasing available stock. |
| **Reservation** | A temporary hold placed on stock when an order is submitted. |
| **Release** | Cancellation of a reservation, returning stock to available. |
| **Deduction** | Permanent removal of reserved stock upon order fulfilment/shipment. |
| **Adjustment** | Authorised manual correction to available stock (cycle count, shrinkage, etc.). |

---

## 3. Domain Model

### 3.1 Aggregate Roots

#### `InventoryItem` ← **Primary Aggregate**

Represents stock tracking for **one SKU** within a tenant. All state mutations are performed exclusively through its public methods, which raise domain events.

```
InventoryItem (AggregateRoot<Guid>, IScoped, IAuditable)
│
├── Id                  : Guid           (identity)
├── ProductId           : Guid           (reference to Catalog)
├── SkuId               : Guid           (reference to variant in Catalog)
├── Sku                 : string         (human-readable SKU code, max 150)
├── StockAvailable      : int            (≥ 0, purchasable quantity)
├── ReservedStock       : int            (≥ 0, held for pending orders)
├── MinimumStock        : int            (≥ 0, low-stock threshold)
│
├── TenantId            : string         (multi-tenancy key — IScoped)
├── Scope               : string         (ring-fence scope)
│
├── CreatedByUserId     : string         (IAuditable)
├── LastModifiedByUserId: string
├── CreatedAtUtc        : DateTimeOffset
└── LastModifiedAtUtc   : DateTimeOffset
```

**Invariants enforced by the aggregate:**

| Rule | Method |
|---|---|
| `StockAvailable` must never be negative | `ReceiveStock`, `AdjustStock` |
| `StockAvailable` must be ≥ requested reservation quantity | `ReserveStock` |
| `ReservedStock` must be ≥ quantity being released or deducted | `ReleaseStock`, `DeductStock` |
| Duplicate inventory for the same `SkuId` is not allowed within a tenant | enforced at command handler / DB unique constraint |

**Lifecycle methods:**

| Method | Trigger | Effect |
|---|---|---|
| `Create(productId, skuId, sku, initialStock, minimumStock)` | VariantCreated event | Creates new tracking record |
| `ReceiveStock(quantity, reason)` | Manual receipt / PO delivery | `StockAvailable += quantity` |
| `ReserveStock(orderId, quantity)` | OrderCreated event | `StockAvailable -= quantity`, `ReservedStock += quantity` |
| `ReleaseStock(orderId, quantity)` | OrderCancelled event | `ReservedStock -= quantity`, `StockAvailable += quantity` |
| `DeductStock(orderId, quantity)` | OrderCompleted / OrderShipped event | `ReservedStock -= quantity` (stock leaves warehouse) |
| `AdjustStock(newAvailableQty, reason)` | Manual audit correction | `StockAvailable = newAvailableQty` |
| `SetMinimumStock(minimumStock)` | Configuration change | `MinimumStock = minimumStock` |

---

#### `Warehouse` ← **Supporting Aggregate** *(future)*

Represents a physical or logical fulfilment location. Currently a thin aggregate; planned to support multi-warehouse stock distribution in future iterations.

```
Warehouse (AggregateRoot<Guid>, IScoped)
│
├── Id       : Guid
├── Name     : string
├── TenantId : string
└── Scope    : string
```

---

### 3.2 Domain Events

Domain events are raised **inside** aggregate methods and persisted to the event store. They represent facts that **have occurred** in the domain.

| Event | Raised By | Key Data |
|---|---|---|
| `InventoryItemCreated` | `InventoryItem.Create()` | `InventoryItemId`, `ProductId`, `SkuId`, `Sku`, `InitialStock`, `MinimumStock` |
| `StockReceived` | `InventoryItem.ReceiveStock()` | `InventoryItemId`, `QuantityReceived`, `NewAvailableStock`, `Reason` |
| `StockReserved` | `InventoryItem.ReserveStock()` | `InventoryItemId`, `OrderId`, `QuantityReserved`, `RemainingAvailableStock` |
| `StockReservationReleased` | `InventoryItem.ReleaseStock()` | `InventoryItemId`, `OrderId`, `QuantityReleased`, `NewAvailableStock` |
| `StockDeducted` | `InventoryItem.DeductStock()` | `InventoryItemId`, `OrderId`, `QuantityDeducted`, `RemainingReservedStock` |
| `StockAdjusted` | `InventoryItem.AdjustStock()` | `InventoryItemId`, `OldAvailableStock`, `NewAvailableStock`, `Reason` |
| `MinimumStockUpdated` | `InventoryItem.SetMinimumStock()` | `InventoryItemId`, `OldMinimumStock`, `NewMinimumStock` |
| `LowStockDetected` | `InventoryItem.ReserveStock()` / `DeductStock()` | `InventoryItemId`, `SkuId`, `CurrentAvailableStock`, `MinimumStock` |

> **Rule:** Domain events are raised inside the aggregate and published internally only. They are **never** published directly to the message bus — that is the responsibility of integration event subscribers.

---

### 3.3 Value Objects

| Value Object | Properties | Validation |
|---|---|---|
| `StockQuantity` | `int Value` | `Value >= 0` |
| `SkuCode` | `string Value` | Non-empty, max 150 chars, uppercase |
| `StockAdjustmentReason` | `string Value` | Non-empty, max 500 chars |

---

### 3.4 Specifications / Invariants

```
SufficientStockSpecification
  → StockAvailable >= requestedQuantity

NoNegativeStockSpecification
  → (StockAvailable + delta) >= 0

ValidReservationReleaseSpecification
  → ReservedStock >= quantityToRelease

UniqueSkuPerTenantSpecification
  → No other InventoryItem with same SkuId exists in tenant (enforced at DB level)
```

---

## 4. Use Cases

### 4.1 Commands

All commands implement `ICommand` and are dispatched via `IMediator`.

| Command | Handler | Description |
|---|---|---|
| `CreateInventoryItemCommand` | `CreateInventoryItemCommandHandler` | Initialize stock tracking for a new SKU (triggered by `VariantCreated` or manual creation). |
| `ReceiveStockCommand` | `ReceiveStockCommandHandler` | Record incoming goods against a specific SKU. |
| `ReserveStockCommand` | `ReserveStockCommandHandler` | Reserve stock for a pending order. Fails if insufficient stock. |
| `ReleaseStockReservationCommand` | `ReleaseStockReservationCommandHandler` | Release previously reserved stock (order cancelled). |
| `DeductStockCommand` | `DeductStockCommandHandler` | Permanently deduct reserved stock (order fulfilled/shipped). |
| `AdjustStockCommand` | `AdjustStockCommandHandler` | Manually correct available stock quantity. |
| `SetMinimumStockCommand` | `SetMinimumStockCommandHandler` | Update the low-stock alert threshold for a SKU. |

**Command Handler Contract (standard pattern):**

```csharp
public sealed class ReserveStockCommandHandler(
    IInventoryItemRepository repository,
    IEventBus eventBus) : ICommandHandler<ReserveStockCommand>
{
    public async Task<Result> HandleAsync(ReserveStockCommand command, CancellationToken cancellationToken)
    {
        // 1. Load aggregate
        var item = await repository.GetBySkuIdAsync(command.SkuId, cancellationToken);
        if (item is null) return Result.Failure(InventoryErrors.NotFound);

        // 2. Mutate — aggregate validates invariants and raises domain events
        var result = item.ReserveStock(command.OrderId, command.Quantity);
        if (result.IsFailure) return result;

        // 3. Persist
        await repository.UpdateAsync(item, cancellationToken);

        // 4. Publish integration event
        await eventBus.PublishAsync<StockReserved>(new { ... }, cancellationToken);

        return Result.Success();
    }
}
```

---

### 4.2 Queries

| Query | Description | Returns |
|---|---|---|
| `GetInventoryItemByIdQuery` | Fetch a single item by its ID | `InventoryItemDto` |
| `GetInventoryItemBySkuIdQuery` | Fetch stock data for a specific SKU | `InventoryItemDto` |
| `GetInventoryItemsByProductIdQuery` | Fetch stock data for all variants of a product | `IReadOnlyList<InventoryItemDto>` |
| `GetLowStockItemsQuery` | Paginated list of items where `StockAvailable <= MinimumStock` | `PagedResult<InventoryItemDto>` |
| `GetInventoryItemsQuery` | Paginated, filterable list of all inventory items | `PagedResult<InventoryItemDto>` |

---

## 5. Integration Events

Integration events are published to **RabbitMQ** via MassTransit and consumed by other services. They represent domain facts with cross-service significance.

All inventory integration events extend `InventoryIntegrationEvent : IntegrationEvent`, which carries:

```csharp
public abstract class InventoryIntegrationEvent : IntegrationEvent
{
    // Inherited: EventId, TimeStampUtc, TenantId, ActionUserId, ActionUserType
}
```

---

### 5.1 Published by Inventory Service

| Event | Trigger | Consumers |
|---|---|---|
| `InventoryItemCreated` | `InventoryItemCreated` domain event | Notification service (optional) |
| `StockReserved` | `StockReserved` domain event | **Order service** — confirms order can proceed |
| `StockReservationFailed` | `ReserveStockCommand` failure (insufficient stock) | **Order service** — must cancel/hold order |
| `StockReservationReleased` | `StockReservationReleased` domain event | Order service (acknowledgement) |
| `StockDeducted` | `StockDeducted` domain event | Order service / Fulfilment service |
| `StockAdjusted` | `StockAdjusted` domain event | Audit / reporting |
| `LowStockAlert` | `LowStockDetected` domain event | Notification service / Purchasing |

**Event contracts (to be added to `EShop.Shared.Contracts/Services/Inventory/`):**

```csharp
// EShop.Shared.Contracts/Services/Inventory/StockReserved.cs
public sealed class StockReserved : InventoryIntegrationEvent
{
    public Guid InventoryItemId { get; init; }
    public Guid OrderId          { get; init; }
    public Guid SkuId            { get; init; }
    public string Sku            { get; init; } = default!;
    public int QuantityReserved  { get; init; }
    public int RemainingStock    { get; init; }
}

// EShop.Shared.Contracts/Services/Inventory/StockReservationFailed.cs
public sealed class StockReservationFailed : InventoryIntegrationEvent
{
    public Guid OrderId          { get; init; }
    public Guid SkuId            { get; init; }
    public string Sku            { get; init; } = default!;
    public int RequestedQuantity { get; init; }
    public int AvailableStock    { get; init; }
    public string Reason         { get; init; } = default!;
}

// EShop.Shared.Contracts/Services/Inventory/LowStockAlert.cs
public sealed class LowStockAlert : InventoryIntegrationEvent
{
    public Guid InventoryItemId     { get; init; }
    public Guid SkuId               { get; init; }
    public string Sku               { get; init; } = default!;
    public int CurrentAvailableStock { get; init; }
    public int MinimumStock         { get; init; }
}
```

---

### 5.2 Consumed by Inventory Service

| Source Service | Integration Event | Consumer | Action |
|---|---|---|---|
| **Catalog** | `VariantCreated` | `VariantCreatedConsumer` | Create `InventoryItem` with `StockAvailable = 0` |
| **Catalog** | `ProductDeleted` | `ProductDeletedConsumer` | Mark all inventory items for the product as inactive |
| **Order** | `OrderCreated` | `OrderCreatedConsumer` | Send `ReserveStockCommand` for each line item |
| **Order** | `OrderCancelled` | `OrderCancelledConsumer` | Send `ReleaseStockReservationCommand` for each line item |
| **Order** | `OrderCompleted` | `OrderCompletedConsumer` | Send `DeductStockCommand` for each line item |

All consumers extend `IdempotentConsumer<TMessage>` to prevent duplicate processing (PostgreSQL inbox pattern).

**Consumer pattern:**

```csharp
internal sealed class VariantCreatedConsumer(IMediator mediator)
    : IdempotentConsumer<VariantCreated>
{
    protected override Task<Result> HandleMessageAsync(
        VariantCreated message,
        CancellationToken cancellationToken)
    {
        var command = new CreateInventoryItemCommand
        {
            ProductId    = message.ProductId,
            SkuId        = message.VariantId,
            Sku          = message.Sku,
            InitialStock = 0,
            MinimumStock = 0
        };
        return mediator.SendAsync(command, cancellationToken);
    }
}
```

---

## 6. Stock Lifecycle

```
                      ┌──────────────────────────┐
                      │   VariantCreated (Catalog)│
                      └────────────┬─────────────┘
                                   │
                                   ▼
                      ┌──────────────────────────┐
                      │   InventoryItem Created   │
                      │   StockAvailable = 0      │
                      │   ReservedStock  = 0      │
                      └────────────┬─────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │     ReceiveStock (PO / API)  │
                    │   StockAvailable += qty      │
                    └──────────────┬──────────────┘
                                   │
           ┌───────────────────────▼───────────────────────┐
           │            OrderCreated (Order svc)            │
           │         ReserveStock → StockAvailable -= qty   │
           │                       ReservedStock  += qty    │
           └──────────┬────────────────────────┬───────────┘
                      │                        │
          ┌───────────▼──────────┐  ┌──────────▼──────────┐
          │  OrderCancelled      │  │  OrderCompleted /    │
          │  ReleaseStock        │  │  OrderShipped        │
          │  ReservedStock -= qty│  │  DeductStock         │
          │  StockAvailable += qty│  │  ReservedStock -= qty│
          │                      │  │  (stock leaves system)│
          └──────────────────────┘  └─────────────────────┘
```

**Stock formula at any point in time:**

```
Physical Stock  = StockAvailable + ReservedStock
Purchasable     = StockAvailable
```

---

## 7. Data Flows

### 7.1 Variant Created → Inventory Initialized

```
Catalog API
    │ Publishes VariantCreated to RabbitMQ
    ▼
MassTransit Consumer: VariantCreatedConsumer
    │ Idempotency check (PostgreSQL InboxMessage)
    │ Calls: CreateInventoryItemCommand via IMediator
    ▼
CreateInventoryItemCommandHandler
    │ Checks: no existing InventoryItem for this SkuId + TenantId
    │ Creates: InventoryItem.Create(productId, skuId, sku, 0, 0)
    │ Aggregate raises: InventoryItemCreated domain event
    │ Persists: EF Core → PostgreSQL
    │ Publishes: InventoryItemCreated integration event
    ▼
InventoryItem record created (StockAvailable=0, ReservedStock=0)
```

---

### 7.2 Order Placed → Stock Reserved

```
Order API
    │ Publishes OrderCreated to RabbitMQ
    │ (contains: OrderId, TenantId, LineItems[{SkuId, Quantity}])
    ▼
MassTransit Consumer: OrderCreatedConsumer
    │ Idempotency check
    │ For each line item:
    │   Calls: ReserveStockCommand(OrderId, SkuId, Quantity)
    ▼
ReserveStockCommandHandler
    │ Loads: InventoryItem by SkuId
    │ Validates: StockAvailable >= Quantity
    │   ├── [INSUFFICIENT] Publishes StockReservationFailed → Order svc
    │   └── [SUFFICIENT]
    │         item.ReserveStock(orderId, quantity)
    │         Aggregate raises: StockReserved domain event
    │         Aggregate checks: StockAvailable <= MinimumStock?
    │           └── Raises: LowStockDetected domain event
    │         Persists to PostgreSQL
    │         Publishes: StockReserved integration event → Order svc
    ▼
Order service receives StockReserved → proceeds to payment
```

---

### 7.3 Order Cancelled → Stock Released

```
Order API
    │ Publishes OrderCancelled to RabbitMQ
    ▼
MassTransit Consumer: OrderCancelledConsumer
    │ Idempotency check
    │ For each line item:
    │   Calls: ReleaseStockReservationCommand(OrderId, SkuId, Quantity)
    ▼
ReleaseStockReservationCommandHandler
    │ Loads: InventoryItem by SkuId
    │ Validates: ReservedStock >= Quantity
    │ item.ReleaseStock(orderId, quantity)
    │ Aggregate raises: StockReservationReleased domain event
    │ Persists to PostgreSQL
    │ Publishes: StockReservationReleased integration event
    ▼
Stock returned to StockAvailable, order safely cancelled
```

---

### 7.4 Order Fulfilled → Stock Deducted

```
Order API
    │ Publishes OrderCompleted to RabbitMQ
    ▼
MassTransit Consumer: OrderCompletedConsumer
    │ Idempotency check
    │ For each line item:
    │   Calls: DeductStockCommand(OrderId, SkuId, Quantity)
    ▼
DeductStockCommandHandler
    │ Loads: InventoryItem by SkuId
    │ Validates: ReservedStock >= Quantity
    │ item.DeductStock(orderId, quantity)
    │ Aggregate raises: StockDeducted domain event
    │ Persists to PostgreSQL
    │ Publishes: StockDeducted integration event
    ▼
Stock permanently removed from system (goods shipped to customer)
```

---

### 7.5 Manual Stock Receipt

```
Warehouse Manager
    │ POST /api/v1/inventory/{id}/receive-stock
    │ Body: { quantity: 50, reason: "PO-12345 delivery" }
    ▼
ReceiveStockCommandHandler
    │ Validates: quantity > 0
    │ item.ReceiveStock(quantity, reason)
    │ Aggregate raises: StockReceived domain event
    │ Persists to PostgreSQL
    │ Publishes: StockReceived integration event (audit trail)
    ▼
StockAvailable increases, item available for sale
```

---

## 8. API Endpoints

Base path: `/api/v1/inventory`

### Write Operations

| Method | Path | Command | Description |
|---|---|---|---|
| `POST` | `/` | `CreateInventoryItemCommand` | Manually initialize stock tracking for a SKU |
| `POST` | `/{id}/receive-stock` | `ReceiveStockCommand` | Record incoming stock from a supplier |
| `POST` | `/{id}/reserve-stock` | `ReserveStockCommand` | Reserve stock for an order (admin/testing use) |
| `POST` | `/{id}/release-reservation` | `ReleaseStockReservationCommand` | Release a stock reservation manually |
| `POST` | `/{id}/deduct-stock` | `DeductStockCommand` | Permanently deduct stock (admin/testing use) |
| `POST` | `/{id}/adjust-stock` | `AdjustStockCommand` | Adjust available stock (audit correction) |
| `PUT` | `/{id}/minimum-stock` | `SetMinimumStockCommand` | Update the low-stock threshold |

### Read Operations

| Method | Path | Query | Description |
|---|---|---|---|
| `GET` | `/{id}` | `GetInventoryItemByIdQuery` | Get stock details by inventory item ID |
| `GET` | `?skuId={skuId}` | `GetInventoryItemBySkuIdQuery` | Get stock details by SKU ID |
| `GET` | `?productId={productId}` | `GetInventoryItemsByProductIdQuery` | Get all variants' stock for a product |
| `GET` | `/low-stock` | `GetLowStockItemsQuery` | Paginated list of items at or below minimum stock |
| `GET` | `/` | `GetInventoryItemsQuery` | Paginated, filterable inventory list |

### Request / Response Examples

**Receive Stock:**
```json
POST /api/v1/inventory/{id}/receive-stock
{
  "quantity": 100,
  "reason": "PO-20260501 — Spring shipment from supplier"
}
```

**Adjust Stock:**
```json
POST /api/v1/inventory/{id}/adjust-stock
{
  "newAvailableQuantity": 45,
  "reason": "Annual cycle count — shrinkage correction"
}
```

**Inventory Item Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "productId": "...",
  "skuId": "...",
  "sku": "SHIRT-RED-M",
  "stockAvailable": 45,
  "reservedStock": 5,
  "physicalStock": 50,
  "minimumStock": 10,
  "isLowStock": false,
  "tenantId": "tenant-abc",
  "createdAtUtc": "2026-01-15T10:30:00Z",
  "lastModifiedAtUtc": "2026-05-05T08:00:00Z"
}
```

---

## 9. Architecture & Project Structure

The service follows **Clean Architecture** with four layers, each enforcing strict dependency rules:

```
Inventory/
├── src/
│   ├── EShop.Inventory.Domain/          ← Layer 1: Core domain (no dependencies)
│   ├── EShop.Inventory.Application/     ← Layer 2: Use cases (depends on Domain)
│   ├── EShop.Inventory.Infrastructure/  ← Layer 3: Persistence, messaging (depends on Application)
│   └── EShop.Inventory.API/             ← Layer 4: HTTP interface (depends on all)
└── test/
    └── EShop.Inventory.Tests/           ← Unit + integration tests
```

### Dependency Rule
```
API → Infrastructure → Application → Domain
                         ↑
                    (no outward deps)
```

### Detailed Project Layout

```
EShop.Inventory.Domain/
├── Entities/
│   ├── InventoryItem.cs            ← Primary aggregate root
│   └── Warehouse.cs                ← Supporting aggregate
├── DomainEvents/
│   ├── InventoryItemCreated.cs
│   ├── StockReceived.cs
│   ├── StockReserved.cs
│   ├── StockReservationReleased.cs
│   ├── StockDeducted.cs
│   ├── StockAdjusted.cs
│   ├── MinimumStockUpdated.cs
│   └── LowStockDetected.cs
├── ValueObjects/
│   ├── StockQuantity.cs
│   ├── SkuCode.cs
│   └── StockAdjustmentReason.cs
├── Abstractions/
│   ├── IInventoryItemRepository.cs
│   └── IInventoryDomainEvent.cs
├── Errors/
│   └── InventoryErrors.cs          ← Domain error definitions
└── ModelConstants.cs

EShop.Inventory.Application/
├── UseCases/
│   ├── Commands/
│   │   ├── CreateInventoryItem/
│   │   │   ├── CreateInventoryItemCommand.cs
│   │   │   └── CreateInventoryItemCommandHandler.cs
│   │   ├── ReceiveStock/
│   │   ├── ReserveStock/
│   │   ├── ReleaseStockReservation/
│   │   ├── DeductStock/
│   │   ├── AdjustStock/
│   │   └── SetMinimumStock/
│   └── Queries/
│       ├── GetInventoryItemById/
│       ├── GetInventoryItemBySkuId/
│       ├── GetInventoryItemsByProductId/
│       ├── GetLowStockItems/
│       └── GetInventoryItems/
└── DependencyInjection/
    └── ServiceCollectionExtensions.cs

EShop.Inventory.Infrastructure/
├── Persistence/
│   ├── InventoryDbContext.cs
│   ├── DbInitializer.cs
│   └── Repositories/
│       └── InventoryItemRepository.cs
├── Configurations/
│   └── InventoryItemEntityTypeConfiguration.cs
├── Consumers/
│   ├── VariantCreatedConsumer.cs
│   ├── ProductDeletedConsumer.cs
│   ├── OrderCreatedConsumer.cs
│   ├── OrderCancelledConsumer.cs
│   └── OrderCompletedConsumer.cs
├── Migrations/
└── DependencyInjection/
    └── ServiceCollectionExtensions.cs

EShop.Inventory.API/
├── Program.cs
├── Startup.cs
├── Endpoints/
│   └── InventoryEndpoints.cs       ← Minimal API endpoint registration
├── DependencyInjection/
│   ├── ServiceCollectionExtensions.cs
│   ├── SwaggerExtensions.cs
│   └── InventorySwaggerOptions.cs
└── Properties/
    └── launchSettings.json
```

---

## 10. Infrastructure

### Database: PostgreSQL

The service uses a **single PostgreSQL database** with EF Core:

| Table | Purpose |
|---|---|
| `inventory` | Persists `InventoryItem` aggregate state |
| `warehouse` | Persists `Warehouse` aggregate state |
| `inbox_messages` | Idempotency store for MassTransit consumers (`IInboxDbContext`) |

**Tenant isolation:** EF Core global query filter on `TenantId` (via `IScoped`). Each `DbContext` instance is scoped to a **single tenant** — never share a context across tenants.

### Message Broker: RabbitMQ via MassTransit

| Direction | Queue | Message |
|---|---|---|
| **Consumed** | `inventory-variant-created` | `VariantCreated` (from Catalog) |
| **Consumed** | `inventory-product-deleted` | `ProductDeleted` (from Catalog) |
| **Consumed** | `inventory-order-created` | `OrderCreated` (from Order) |
| **Consumed** | `inventory-order-cancelled` | `OrderCancelled` (from Order) |
| **Consumed** | `inventory-order-completed` | `OrderCompleted` (from Order) |
| **Published** | Exchange: `stock-reserved` | `StockReserved` |
| **Published** | Exchange: `stock-reservation-failed` | `StockReservationFailed` |
| **Published** | Exchange: `stock-reservation-released` | `StockReservationReleased` |
| **Published** | Exchange: `stock-deducted` | `StockDeducted` |
| **Published** | Exchange: `low-stock-alert` | `LowStockAlert` |

**Retry policy:** Incremental backoff (3 attempts: 5s, 15s, 30s) with dead-letter queue on final failure.

### Idempotency (Inbox Pattern)

All consumers extend `IdempotentConsumer<TMessage>`:
1. Check PostgreSQL `inbox_messages` table for `MessageId`
2. If found → skip (already processed)
3. If not found → process and insert record atomically

This ensures **exactly-once processing** semantics even if RabbitMQ delivers a message twice.

### Observability

| Concern | Implementation |
|---|---|
| Structured logging | Serilog with OpenTelemetry sink |
| Distributed tracing | OpenTelemetry (OTLP → Grafana Tempo) |
| Metrics | OpenTelemetry metrics → Prometheus → Grafana |
| Health checks | `/health/live`, `/health/ready` via `EShop.ServiceDefaults` |

---

## 11. Configuration

### Application Settings (`appsettings.json`)

```jsonc
{
  "ConnectionStrings": {
    "inventoryDatabase": "Host=localhost;Database=inventory_db;Username=postgres;Password=..."
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "..."
  },
  "Inventory": {
    "DefaultMinimumStock": 5
  }
}
```

### Environment Variables (Docker / Aspire)

| Variable | Description |
|---|---|
| `ConnectionStrings__inventoryDatabase` | PostgreSQL connection string |
| `RabbitMQ__Host` | RabbitMQ host |
| `RabbitMQ__Username` | RabbitMQ username |
| `RabbitMQ__Password` | RabbitMQ password (use Docker secret) |

---

## 12. Testing Strategy

### Unit Tests (`EShop.Inventory.Tests`)

| Target | Test Type | Tools |
|---|---|---|
| Aggregate methods (invariants) | Unit | xUnit + FluentAssertions |
| Command handlers | Unit | xUnit + FakeItEasy (mock repository + event bus) |
| Domain event raising | Unit | xUnit |
| Specification classes | Unit | xUnit |

**Example — aggregate invariant test:**

```csharp
public class InventoryItem_ReserveStock_Tests
{
    [Fact]
    public void ReserveStock_WhenSufficientStock_ShouldDecreaseAvailableAndIncreaseReserved()
    {
        // Arrange
        var item = InventoryItemFaker.CreateWithStock(stockAvailable: 10, reservedStock: 0);

        // Act
        var result = item.ReserveStock(orderId: Guid.NewGuid(), quantity: 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        item.StockAvailable.Should().Be(7);
        item.ReservedStock.Should().Be(3);
    }

    [Fact]
    public void ReserveStock_WhenInsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var item = InventoryItemFaker.CreateWithStock(stockAvailable: 2, reservedStock: 0);

        // Act
        var result = item.ReserveStock(orderId: Guid.NewGuid(), quantity: 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InventoryErrors.InsufficientStock);
    }

    [Fact]
    public void ReserveStock_WhenStockFallsBelowMinimum_ShouldRaiseLowStockDetectedEvent()
    {
        // Arrange
        var item = InventoryItemFaker.CreateWithStock(stockAvailable: 3, minimumStock: 5);

        // Act
        item.ReserveStock(orderId: Guid.NewGuid(), quantity: 1);

        // Assert
        item.DomainEvents.Should().ContainSingle(e => e is LowStockDetected);
    }
}
```

### BDD Tests (Reqnroll.xUnit)

Key scenarios to cover:

```gherkin
Feature: Stock Reservation

  Scenario: Successfully reserve stock for an order
    Given a SKU "SHIRT-RED-M" with 20 units available
    When an order is placed for 5 units of "SHIRT-RED-M"
    Then 15 units should be available
    And 5 units should be reserved
    And a StockReserved integration event should be published

  Scenario: Fail reservation when stock is insufficient
    Given a SKU "SHIRT-RED-M" with 3 units available
    When an order is placed for 5 units of "SHIRT-RED-M"
    Then the reservation should fail
    And a StockReservationFailed integration event should be published
    And the available stock should remain 3

  Scenario: Trigger low-stock alert after reservation
    Given a SKU "SHIRT-RED-M" with 6 units available and minimum stock of 5
    When an order is placed for 2 units of "SHIRT-RED-M"
    Then 4 units should be available
    And a LowStockAlert integration event should be published
```

---

*This document is the authoritative design reference for the Inventory microservice. Implementation tasks are tracked in the OpenSpec change workflow (`openspec/changes/`).*
