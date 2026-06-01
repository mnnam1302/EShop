## 1. Infrastructure & Database Migrations

- [ ] 1.1 Add Redis connection (`IConnectionMultiplexer`) to Inventory service DI in `ServiceCollectionExtensions`
- [ ] 1.2 Add `RowVersion` (concurrency token) column to `Inventories` table via new EF Core migration in `EShop.Inventory.Infrastructure`
- [ ] 1.3 Create `StockReservations` table via EF Core migration: Id, ReservationGroupId, InventoryId, OrderId, IdempotencyKey, Quantity, Status, ReservedAt, ExpiresAt, ConfirmedAt, ReleasedAt
- [ ] 1.4 Add UNIQUE INDEX on `StockReservations.IdempotencyKey` (partial: WHERE Status != 'Released') in migration
- [ ] 1.5 Add INDEX on `StockReservations(Status, ExpiresAt)` for expiration job query performance
- [ ] 1.6 Create `SagaStates` table via EF Core migration in `EShop.Order.Infrastructure`: CorrelationId, CurrentState, OrderId, BuyerId, Items (jsonb), ReservationId, SubmittedAt, ReservedAt, CompletedAt, FailureReason, StockReservationTimeoutTokenId
- [ ] 1.7 Add INDEX on `SagaStates(OrderId)` and `SagaStates(CurrentState)` in migration
- [ ] 1.8 Add Hangfire to Inventory service DI and configure recurring job schedule

## 2. Shared Contracts (Integration Events & Commands)

- [ ] 2.1 Define `ReserveStockCommand` in `EShop.Shared.Contracts`: OrderId, IdempotencyKey, Items (VariantId, Quantity)
- [ ] 2.2 Define `ReleaseReservationCommand` in `EShop.Shared.Contracts`: OrderId, ReservationId (nullable)
- [ ] 2.3 Define `ConfirmReservationCommand` in `EShop.Shared.Contracts`: OrderId, ReservationId
- [ ] 2.4 Define `PersistOrderCommand` in `EShop.Shared.Contracts`: OrderId, BuyerId, Items, ReservationId
- [ ] 2.5 Define `OrderSubmitted` integration event: OrderId, BuyerId, Items, SubmittedAt
- [ ] 2.6 Define `StockReserved` integration event: OrderId, ReservationId, ReservedAt
- [ ] 2.7 Define `StockReservationFailed` integration event: OrderId, Reason
- [ ] 2.8 Define `ReservationExpired` integration event: OrderId, ReservationId, ExpiredAt
- [ ] 2.9 Define `OrderRejected` integration event: OrderId, Reason, RejectedAt
- [ ] 2.10 Define `OrderPersisted` integration event: OrderId, PersistedAt

## 3. Redis Stock Gateway (Inventory Service)

- [ ] 3.1 Create `RedisStockGateway` class in `EShop.Inventory.Infrastructure` with `IConnectionMultiplexer` injection
- [ ] 3.2 Implement `CheckAndReserveAsync(items)` — multi-item atomic Lua script: Phase 1 checks all available, Phase 2 reserves all; returns `(bool Success, Guid? FailedVariantId)`
- [ ] 3.3 Implement `CompensateAsync(items)` — Lua script to atomically restore stock (INCRBY available, DECRBY reserved) for all items
- [ ] 3.4 Implement `SyncFromPostgresAsync(inventories)` — batch SET for `stock:available:{variantId}` and `stock:reserved:{variantId}` using Redis pipeline
- [ ] 3.5 Implement `IsInitializedAsync()` — check existence of `stock:_initialized` sentinel key
- [ ] 3.6 Implement `MarkInitializedAsync()` — set `stock:_initialized` sentinel key
- [ ] 3.7 Implement circuit breaker wrapper: detect Redis connectivity failure, open circuit after 30s of failures, emit warning metric
- [ ] 3.8 Register `RedisStockGateway` as `IRedisStockGateway` scoped service in DI

## 4. Redis Cache Initialization & Background Sync (Inventory Service)

- [ ] 4.1 Create `RedisStockInitializer : IHostedService` — on `StartAsync`, call `IsInitializedAsync()`, if false load all Inventories from Postgres and call `SyncFromPostgresAsync`, then `MarkInitializedAsync`
- [ ] 4.2 Register `RedisStockInitializer` as hosted service in Inventory service startup
- [ ] 4.3 Create `SyncRedisStockJob` (Hangfire) — load recently modified Inventories (LastModifiedAtUtc > UtcNow - 5 min), call `SyncFromPostgresAsync`, log divergences > 10 units threshold
- [ ] 4.4 Create `ExpireReservationsJob` (Hangfire) — query `StockReservations WHERE Status='Pending' AND ExpiresAt < UtcNow`, release each: update Postgres (Available++, Reserved--), call `CompensateAsync` for Redis, publish `ReservationExpired` event
- [ ] 4.5 Register `SyncRedisStockJob` as Hangfire recurring job (every 5 minutes)
- [ ] 4.6 Register `ExpireReservationsJob` as Hangfire recurring job (every 1 minute)

## 5. Inventory Reservation Command Handlers

- [ ] 5.1 Create `StockReservation` entity in `EShop.Inventory.Domain` with all required fields and `ReservationStatus` enum (Pending, Confirmed, Released)
- [ ] 5.2 Add `DbSet<StockReservation> StockReservations` to `InventoryDbContext` with EF Core configuration
- [ ] 5.3 Create `ReserveStockCommandHandler : IConsumer<ReserveStockCommand>` in `EShop.Inventory.Application`:
  - Step 1: Idempotency check — query StockReservations by IdempotencyKey; return existing StockReserved if found
  - Step 2: Call `IRedisStockGateway.CheckAndReserveAsync`; publish StockReservationFailed if returns false
  - Step 3: Begin Postgres transaction; foreach item verify StockAvailable >= Quantity (compensate Redis + publish failure if not); deduct stock; insert StockReservation records; commit
  - Step 4: On `DbUpdateConcurrencyException`, call `CompensateAsync` and rethrow (MassTransit retry handles)
  - Step 5: Publish `StockReserved` event with ReservationId
- [ ] 5.4 Create `ReleaseReservationCommandHandler : IConsumer<ReleaseReservationCommand>` — idempotent: no-op if no reservation or already Released; else update Inventory (Available++, Reserved--), set Status=Released, call `CompensateAsync`, publish nothing
- [ ] 5.5 Create `ConfirmReservationCommandHandler : IConsumer<ConfirmReservationCommand>` — idempotent: no-op if already Confirmed; return failure if Released/Expired; set Status=Confirmed, ConfirmedAt=UtcNow
- [ ] 5.6 Register all three consumers in MassTransit configuration in Inventory service

## 6. Place Order Saga State Machine (Order Service)

- [ ] 6.1 Add `MassTransit.EntityFrameworkCore` NuGet package to `EShop.Order.Infrastructure`
- [ ] 6.2 Create `PlaceOrderSagaState : SagaStateMachineInstance` in `EShop.Order.Application` with all state fields (CorrelationId, CurrentState, OrderId, BuyerId, Items, ReservationId, SubmittedAt, ReservedAt, CompletedAt, FailureReason, StockReservationTimeoutTokenId)
- [ ] 6.3 Add EF Core `SagaStateConfiguration` (entity type config) and `DbSet<PlaceOrderSagaState>` to Order DbContext
- [ ] 6.4 Create `PlaceOrderSaga : MassTransitStateMachine<PlaceOrderSagaState>` in `EShop.Order.Application`:
  - Declare states: `StockChecking`, `StockReserved`, `Completed`, `Failed`, `Compensating`
  - Declare events: `OrderSubmitted`, `StockReserved`, `StockReservationFailed`, `OrderPersisted`, `OrderCancelled`
  - Declare schedule: `StockReservationTimeout` (30 seconds)
  - `Initially`: on `OrderSubmitted` → save data, transition to `StockChecking`, send `ReserveStockCommand`, schedule timeout
  - `During(StockChecking)`: on `StockReserved` → unschedule timeout, save ReservationId, transition to `StockReserved`, send `PersistOrderCommand`
  - `During(StockChecking)`: on `StockReservationFailed` → unschedule timeout, transition to `Failed`, publish `OrderRejected`
  - `During(StockChecking)`: on timeout → transition to `Compensating`, send `ReleaseReservationCommand`, publish `OrderRejected`
  - `During(StockReserved)`: on `OrderPersisted` → send `ConfirmReservationCommand`, transition to `Completed`, `Finalize()`
  - `DuringAny`: on `OrderCancelled` → transition to `Compensating`, send `ReleaseReservationCommand`, publish `OrderCancelled` event
  - Call `SetCompletedWhenFinalized()`
- [ ] 6.5 Register `PlaceOrderSaga` with MassTransit Entity Framework repository (optimistic concurrency) in Order service DI
- [ ] 6.6 Configure MassTransit retry policy: exponential backoff (1s, 2s, 4s, 8s, 16s, max 5 retries) for all consumers and saga

## 7. Order Service Consumers

- [ ] 7.1 Create `PersistOrderCommandHandler : IConsumer<PersistOrderCommand>` in `EShop.Order.Application`:
  - Check idempotency: if Order with same OrderId exists, publish `OrderPersisted` (idempotent)
  - Create `Order` aggregate via `Order.CreateOrder(...)`
  - Persist to Order repository and SaveChanges
  - Publish `OrderPersisted` event
- [ ] 7.2 Register `PersistOrderCommandHandler` in MassTransit configuration in Order service

## 8. PlaceOrderCommandHandler Refactor

- [ ] 8.1 Update `PlaceOrderCommandHandler` to publish `OrderSubmitted` integration event via `IEventBus` instead of directly persisting the order
- [ ] 8.2 Return `Result.Success(new { OrderId = orderId })` with the saga CorrelationId so the API can return it to the client
- [ ] 8.3 Update `PlaceOrder` API endpoint to return `202 Accepted` with `{ orderId }` response body

## 9. Testing

- [ ] 9.1 Unit test: `RedisStockGateway` — Lua check-and-reserve returns success when sufficient stock; returns failed VariantId when insufficient; modifies no keys on failure
- [ ] 9.2 Unit test: `RedisStockGateway` — compensation Lua script restores all keys for all items
- [ ] 9.3 Unit test: `ReserveStockCommandHandler` — idempotency: second call with same IdempotencyKey returns existing ReservationId without modifying stock
- [ ] 9.4 Unit test: `ReserveStockCommandHandler` — Redis stale data: Redis passes, Postgres rejects, Redis is compensated, StockReservationFailed published
- [ ] 9.5 Unit test: `ReserveStockCommandHandler` — `DbUpdateConcurrencyException` triggers Redis compensation and rethrows for retry
- [ ] 9.6 Unit test: `ReleaseReservationCommandHandler` — no-op on non-existent OrderId; no-op on already Released reservation; releases Pending reservation correctly
- [ ] 9.7 Unit test: `ConfirmReservationCommandHandler` — idempotent on already Confirmed; fails on Released reservation
- [ ] 9.8 Unit test: `PlaceOrderSaga` state transitions — happy path (Initial → StockChecking → StockReserved → Completed)
- [ ] 9.9 Unit test: `PlaceOrderSaga` — `StockReservationFailed` transitions to `Failed` and publishes `OrderRejected`
- [ ] 9.10 Unit test: `PlaceOrderSaga` — timeout transitions to `Compensating`, sends `ReleaseReservationCommand`, publishes `OrderRejected`
- [ ] 9.11 Unit test: `ExpireReservationsJob` — releases expired reservations, skips non-expired ones
- [ ] 9.12 Integration test: `ReserveStockCommandHandler` with real PostgreSQL — 100 concurrent reservations for same variant; final StockAvailable equals initial minus total successfully reserved
- [ ] 9.13 Integration test: full saga flow with MassTransit test harness — happy path, stock failure path, timeout path
- [ ] 9.14 Integration test: Redis fallback mode — Redis unavailable, orders still process via Postgres-only path

## 10. Monitoring & Observability

- [ ] 10.1 Add OpenTelemetry counter: `saga.place_order.submitted` — incremented on each `OrderSubmitted`
- [ ] 10.2 Add OpenTelemetry counter: `saga.place_order.completed` — incremented on saga `Completed`
- [ ] 10.3 Add OpenTelemetry counter: `saga.place_order.failed` — incremented on saga `Failed` (tagged by reason)
- [ ] 10.4 Add OpenTelemetry histogram: `saga.place_order.duration_ms` — duration from `SubmittedAt` to `CompletedAt`
- [ ] 10.5 Add warning log in `SyncRedisStockJob` when Redis-Postgres divergence exceeds 10 units threshold
- [ ] 10.6 Add warning metric `redis.stock.fallback_active` when circuit breaker is open (Redis unavailable)
- [ ] 10.7 Add health check for Redis connectivity in Inventory service health check endpoint
