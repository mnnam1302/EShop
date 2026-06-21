## ADDED Requirements

### Requirement: Atomic conditional stock deduction (no oversell)

The system SHALL deduct stock using a single atomic conditional UPDATE guarded by `stock_available >= quantity`, such that two concurrent orders can never both succeed against the last available unit.

#### Scenario: Sufficient stock deducts successfully
- **WHEN** a reservation request is processed for a variant whose `stock_available` is greater than or equal to the requested quantity
- **THEN** the system SHALL decrement `stock_available` by the requested quantity in one atomic statement and report the deduction as successful

#### Scenario: Insufficient stock is rejected
- **WHEN** a reservation request is processed for a variant whose `stock_available` is less than the requested quantity
- **THEN** the conditional UPDATE SHALL affect zero rows and the system SHALL NOT decrement stock for that variant

#### Scenario: Concurrent orders contend for the last unit
- **WHEN** two orders concurrently request the only remaining unit of a variant
- **THEN** exactly one order SHALL succeed and the other SHALL be rejected, and `stock_available` SHALL never become negative

### Requirement: Redis fast-gate precedes the database

The system SHALL evaluate an atomic Redis Lua fast-gate (all-or-nothing across all requested items) before opening the database transaction, and SHALL compensate the Redis gate when the database deduction subsequently fails.

#### Scenario: Gate rejects an out-of-stock request cheaply
- **WHEN** the Redis fast-gate reports insufficient stock for any requested item
- **THEN** the system SHALL reject the request without opening a database transaction

#### Scenario: Gate passes but database rejects
- **WHEN** the Redis fast-gate passes but a subsequent CAS deduction affects zero rows
- **THEN** the system SHALL release the previously gated quantities back to Redis so the cache returns to the database-authoritative value

#### Scenario: Cache miss warms from the database
- **WHEN** the Redis fast-gate has no entry for a requested variant
- **THEN** the system SHALL seed the Redis available count from the database before retrying the gate

### Requirement: Idempotent deduction independent of CAS

The system SHALL guarantee exactly-once-effect deduction per order under at-least-once message delivery, using an inbox dedupe and a `UNIQUE(tenant_id, order_id)` reservation guard committed in the same transaction as the deduction. CAS alone MUST NOT be relied upon for idempotency.

#### Scenario: Redelivered reservation message does not double-deduct
- **WHEN** the same `MakeReservation` message for an order is delivered more than once
- **THEN** the system SHALL deduct stock for that order exactly once and SHALL treat subsequent deliveries as already-processed

#### Scenario: Concurrent duplicate deliveries are serialized by the unique guard
- **WHEN** two deliveries of the same order's reservation are processed concurrently
- **THEN** the `UNIQUE(tenant_id, order_id)` constraint SHALL cause exactly one to commit and the other to be acknowledged and skipped without a second deduction

### Requirement: Deterministic multi-item ordering and deadlock handling

The system SHALL acquire inventory row locks for a multi-item order in ascending `VariantId` order, and SHALL retry a bounded number of times if the database reports a deadlock.

#### Scenario: Opposite item orders do not deadlock
- **WHEN** one order requests items `[A, B]` and a concurrent order requests `[B, A]`
- **THEN** both transactions SHALL acquire locks in the same `VariantId` order and SHALL NOT form a deadlock cycle

#### Scenario: Transient deadlock is retried
- **WHEN** the database aborts a deduction transaction with a deadlock error
- **THEN** the system SHALL retry the transaction up to the configured bound before failing the order

### Requirement: All-or-nothing multi-item reservation

The system SHALL reserve all items of an order or none; a shortfall on any single item SHALL fail the entire order and leave no partial deduction.

#### Scenario: One item short fails the whole order
- **WHEN** an order contains multiple items and at least one item has insufficient stock
- **THEN** the system SHALL roll back the database transaction, compensate any Redis gate deductions, and publish a reservation failure for the order

### Requirement: Hold and reservation items persisted with the deduction

The system SHALL, within the same transaction as the deduction, create a per-order `Reservation` in `Pending` state with a 15-minute expiry and one `ReservationItem` row per variant capturing the reserved quantity.

#### Scenario: Successful reservation records the hold and line items
- **WHEN** a multi-item order is deducted successfully
- **THEN** the committed transaction SHALL contain one `Reservation` row (status `Pending`, `ExpiresAt` set to 15 minutes ahead) and one `ReservationItem` row per requested variant with its quantity

#### Scenario: Hold creation shares the deduction transaction
- **WHEN** the deduction transaction fails for any reason after the CAS updates
- **THEN** neither the stock decrements nor the `Reservation`/`ReservationItem` rows SHALL persist
