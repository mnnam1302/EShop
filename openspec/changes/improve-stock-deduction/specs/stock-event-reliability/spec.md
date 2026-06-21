## ADDED Requirements

### Requirement: Transactional outbox for reservation events

The system SHALL write `StockReserved` and `StockReservationFailed` integration events into an outbox table within the same transaction as the stock change, and SHALL publish them to the message broker only after that transaction commits.

#### Scenario: Event survives a crash after commit
- **WHEN** the deduction transaction commits but the process crashes before the event reaches the broker
- **THEN** the outbox relay SHALL publish the persisted event after recovery, so the saga is not stranded

#### Scenario: No event is published when the transaction rolls back
- **WHEN** the deduction transaction rolls back
- **THEN** no `StockReserved` event SHALL be published for that order

### Requirement: StockReserved carries the reservation identifier

The system SHALL include the `ReservationId` on every `StockReserved` event so the saga can thread it into `PersistOrderCommand` and any later release.

#### Scenario: Reservation id is propagated to the saga
- **WHEN** a `StockReserved` event is published for an order
- **THEN** it SHALL contain the `ReservationId` of the hold created for that order

### Requirement: Polling publisher relay

The system SHALL relay outbox events with a polling publisher that selects unprocessed rows, publishes them, and marks them processed, in a way that is safe to run on multiple instances and tolerant of duplicate broker delivery.

#### Scenario: Pending events are published and marked processed
- **WHEN** the relay finds outbox rows with no processed timestamp
- **THEN** it SHALL publish each to the broker and set its processed timestamp

#### Scenario: Multiple relay instances do not double-publish
- **WHEN** more than one relay instance polls the outbox concurrently
- **THEN** each pending row SHALL be claimed and published by at most one instance

### Requirement: Redis and Postgres reconciliation

The system SHALL treat PostgreSQL as the source of truth and SHALL periodically reseed the Redis available counters from PostgreSQL to heal any drift, such that drift can cause a transient lost sale but never an oversell.

#### Scenario: Drift after a failed compensation is healed
- **WHEN** the Redis available count diverges from the PostgreSQL value after an interrupted compensation
- **THEN** the reconciliation job SHALL reseed the Redis count from PostgreSQL

#### Scenario: Postgres remains authoritative under drift
- **WHEN** Redis under-reports availability before reconciliation runs
- **THEN** the database CAS guard SHALL still prevent any oversell
