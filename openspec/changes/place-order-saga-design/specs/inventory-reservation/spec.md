## ADDED Requirements

### Requirement: Stock reservation is idempotent using IdempotencyKey
The system SHALL check for an existing StockReservation record matching the provided IdempotencyKey before performing any reservation operations. If a matching record exists, the system SHALL return the existing ReservationId without performing any further stock operations. The IdempotencyKey is the saga CorrelationId and SHALL be stored in a column with a UNIQUE index.

#### Scenario: First reservation request processes normally
- **WHEN** a ReserveStockCommand is received with IdempotencyKey that has no existing reservation
- **THEN** the system proceeds with Redis fast-gate and Postgres reservation
- **AND** a StockReservation record is inserted with that IdempotencyKey

#### Scenario: Duplicate reservation command returns existing result
- **WHEN** a ReserveStockCommand is received with an IdempotencyKey that already exists in StockReservations
- **THEN** the system returns StockReserved with the existing ReservationId
- **AND** no additional stock is deducted from Redis or Postgres

#### Scenario: Concurrent duplicate commands only process one
- **WHEN** two ReserveStockCommands with the same IdempotencyKey arrive simultaneously
- **THEN** only one succeeds in inserting the StockReservation record
- **AND** the other receives a unique constraint violation and returns the existing record
- **AND** total stock deducted equals the requested quantity (not doubled)

---

### Requirement: Redis Lua script atomically checks and reserves stock before Postgres write
The system SHALL execute a Redis Lua script to atomically check available stock and tentatively reserve it for all ordered items before attempting Postgres persistence. If any item has insufficient stock in Redis, the script SHALL reject the entire order and NO stock SHALL be modified in Redis. The Lua script SHALL execute as a single atomic operation (no interleaving with other operations).

#### Scenario: All items have sufficient stock - Redis reserves successfully
- **WHEN** all ordered items have sufficient `stock:available:{variantId}` in Redis
- **THEN** the Lua script atomically decrements `stock:available` and increments `stock:reserved` for each item
- **AND** returns success, allowing Postgres persistence to proceed

#### Scenario: One item has insufficient stock - entire reservation rejected
- **WHEN** at least one ordered item has `stock:available:{variantId}` less than requested quantity
- **THEN** the Lua script rejects without modifying any Redis keys
- **AND** a StockReservationFailed event is published with the failing VariantId

#### Scenario: Multi-item reservation is all-or-nothing in Redis
- **WHEN** a 3-item order is processed and items 1 and 2 pass the check but item 3 fails
- **THEN** no stock is reserved for items 1, 2, or 3
- **AND** Redis state is unchanged

---

### Requirement: Postgres transaction persists reservation as authoritative source of truth
The system SHALL persist the stock reservation to PostgreSQL within a single database transaction that atomically updates the `Inventories` table (decrement StockAvailable, increment ReservedStock) and inserts a `StockReservations` record. Postgres is the authoritative source of truth. If Postgres rejects the reservation (e.g., stale Redis data), the system SHALL compensate Redis by restoring the tentatively reserved stock.

#### Scenario: Postgres confirms sufficient stock and commits reservation
- **WHEN** the Postgres transaction begins after Redis fast-gate succeeds
- **THEN** inventory rows are read and verified for sufficient StockAvailable
- **AND** StockAvailable is decremented and ReservedStock is incremented within the transaction
- **AND** a StockReservation record is inserted with Status = Pending, ExpiresAt = UtcNow + 15 minutes
- **AND** the transaction is committed

#### Scenario: Postgres detects stale Redis data and compensates
- **WHEN** the Postgres authoritative check finds StockAvailable less than requested quantity (Redis was stale)
- **THEN** the Postgres transaction is rolled back
- **AND** the Redis compensation Lua script restores the tentatively reserved stock
- **AND** a StockReservationFailed event is published with reason "Insufficient stock (authoritative)"

#### Scenario: Optimistic concurrency conflict triggers retry
- **WHEN** two concurrent reservations for the same inventory row both attempt to commit
- **THEN** one commit succeeds and the other raises a DbUpdateConcurrencyException
- **AND** the failing handler compensates Redis and retries via MassTransit retry policy
- **AND** the retry re-evaluates both Redis and Postgres stock levels

---

### Requirement: Reservation records have a 15-minute expiration timeout
Each StockReservation record SHALL include a mandatory `ExpiresAt` timestamp set to `UtcNow + 15 minutes` at creation time. Reservations with Status = Pending and ExpiresAt < UtcNow are considered expired and SHALL be released by the background expiration job.

#### Scenario: Reservation created with correct expiration timestamp
- **WHEN** a StockReservation is inserted
- **THEN** ExpiresAt is set to exactly UtcNow + 15 minutes
- **AND** Status is set to Pending

#### Scenario: Reservation is not released before expiration
- **WHEN** a reservation exists with Status = Pending and ExpiresAt in the future
- **THEN** the background expiration job does NOT release it

#### Scenario: Expired reservation is released by background job
- **WHEN** a reservation has Status = Pending and ExpiresAt < UtcNow
- **THEN** the background expiration job releases the reservation
- **AND** StockAvailable is incremented and ReservedStock is decremented in Postgres
- **AND** Redis is updated to reflect the released stock
- **AND** a ReservationExpired integration event is published

---

### Requirement: Confirmed reservation finalises the stock deduction
The system SHALL support a ConfirmReservation operation that transitions a StockReservation from Status = Pending to Status = Confirmed. A confirmed reservation MUST NOT be released by the expiration job. Confirmation is called by the saga after the order is successfully persisted.

#### Scenario: Pending reservation is confirmed
- **WHEN** a ConfirmReservationCommand is received for a valid Pending reservation
- **THEN** the StockReservation Status is updated to Confirmed
- **AND** ConfirmedAt is set to UtcNow
- **AND** no stock is returned to available (stock deduction is finalised)

#### Scenario: Already confirmed reservation is idempotent
- **WHEN** a ConfirmReservationCommand is received for a reservation that is already Confirmed
- **THEN** the operation succeeds without error
- **AND** no stock values are changed

#### Scenario: Expired reservation cannot be confirmed
- **WHEN** a ConfirmReservationCommand is received for a reservation with Status = Released or Expired
- **THEN** the system returns a failure result
- **AND** an OrderRejected compensation flow is triggered

---

### Requirement: Background expiration job runs every 1 minute
The system SHALL run a Hangfire recurring job every 1 minute that finds and releases all StockReservations where Status = Pending AND ExpiresAt < UtcNow. Each release SHALL update Postgres and Redis atomically, and publish a ReservationExpired event per expired reservation.

#### Scenario: Expiration job processes multiple expired reservations in one execution
- **WHEN** 50 reservations have expired since the last job run
- **THEN** all 50 are processed in a single job execution
- **AND** Postgres and Redis are updated for each
- **AND** 50 ReservationExpired events are published
