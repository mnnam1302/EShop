## Why

The current PlaceOrderCommandHandler persists orders without verifying inventory availability or coordinating distributed transactions across Order and Inventory services. Under high concurrency, this leads to overselling, race conditions on stock deduction, and lack of failure recovery. Production systems require saga-based orchestration with idempotent operations and Redis-backed fast-path stock checking to ensure consistency and prevent data corruption.

## What Changes

- Implement **Saga orchestration pattern** (choreography-based) for distributed order placement transactions across Order, Inventory, and future Finance services
- Add **Redis Lua-based fast gate** for high-concurrency stock availability checks before database operations (prevents contention)
- Implement **idempotent inventory reservation** with deduplication keys to handle retries and duplicate requests safely
- Create **compensation logic** for saga rollback when inventory reservation or payment (future) fails
- Add **saga state persistence** using MassTransit state machine with PostgreSQL storage
- Integrate **outbox pattern** for reliable event publishing from Order and Inventory aggregates
- Extend **OrderStatus** state machine to include saga-specific states (InventoryReserved, PaymentProcessing, etc.)

## Capabilities

### New Capabilities

- `order-saga-orchestration`: Saga state machine using MassTransit Automatonymous for orchestrating Place Order workflow across services, including state persistence, timeout handling, and saga instance correlation
- `inventory-reservation`: High-concurrency atomic stock checking and reservation with Redis-based fast gate (Lua script), idempotency guards using deduplication keys, optimistic locking on database, and reservation timeout/expiration
- `saga-compensation`: Compensation logic for rolling back partial transactions including inventory release, order cancellation, and audit trail of compensation events
- `redis-stock-gateway`: Redis-based fast-path stock availability verification using Lua scripts for atomic multi-item checks, with fallback to database for consistency verification

### Modified Capabilities

_(None - this is greenfield saga implementation)_

## Impact

**Affected Services:**
- **Order service**: Add saga participant behavior, outbox event publishing, new domain events (OrderPlaced, OrderConfirmed, OrderRejected, OrderCancelled), saga state transitions
- **Inventory service**: Implement ReserveStock/ReleaseStock commands with idempotency, add domain events (StockReserved, StockReleased, ReservationExpired), optimistic concurrency control
- **Shared contracts**: New integration events for saga choreography (OrderPlacedEvent, InventoryReservationRequested, InventoryReservationCompleted, InventoryReservationFailed)

**Infrastructure:**
- **Redis**: Deploy Redis for stock availability cache and Lua script execution (requires redis-stack or similar for Lua support)
- **Saga state store**: PostgreSQL table for saga state persistence (MassTransit state machine storage)
- **MassTransit**: Configure saga orchestration, state machine, message routing, and timeout scheduling

**Database Changes:**
- Order service: Add saga correlation fields (CorrelationId, SagaState) to Order aggregate
- Inventory service: Add ReservationId, ReservedAt, ReservationExpiresAt columns to Inventory entity
- New saga state table: SagaStates (managed by MassTransit)

**APIs:**
- PlaceOrder endpoint: Update to return saga correlation ID for tracking
- New query endpoint: GetOrderStatus by correlation ID to check saga progress

**Testing:**
- High-concurrency stress tests for Redis stock gateway (10K+ concurrent requests)
- Saga compensation scenario tests (inventory failure, timeout scenarios)
- Idempotency tests (duplicate PlaceOrderCommand with same deduplication key)
- Integration tests across Order and Inventory services with saga orchestration

**Future Extensibility:**
- Saga prepared for Finance service integration (payment processing step)
- Designed for eventual 2PC-like consistency with compensating transactions
