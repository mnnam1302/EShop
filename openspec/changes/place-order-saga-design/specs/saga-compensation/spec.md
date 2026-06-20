## ADDED Requirements

### Requirement: Saga triggers compensation when stock reservation fails
The system SHALL trigger compensation immediately upon receiving a StockReservationFailed event while the saga is in the `StockChecking` state. Compensation SHALL cancel the order and publish an OrderRejected event. No partial compensations are required for this state (no stock was reserved).

#### Scenario: StockReservationFailed in StockChecking triggers immediate rejection
- **WHEN** the saga is in `StockChecking` state and receives StockReservationFailed
- **THEN** the saga transitions to `Failed`
- **AND** an OrderRejected integration event is published containing the OrderId and failure reason
- **AND** no ReleaseReservationCommand is sent (nothing was reserved)

---

### Requirement: Saga triggers compensation when stock reservation times out
The system SHALL trigger compensation when the stock reservation timeout expires while the saga is in the `StockChecking` state. The compensation SHALL send a ReleaseReservationCommand to release any partial reservation that may have been created before the timeout response was received, and SHALL publish an OrderRejected event.

#### Scenario: Timeout fires and sends release command
- **WHEN** the saga is in `StockChecking` state and the 30-second timeout expires
- **THEN** the saga transitions to `Compensating`
- **AND** a ReleaseReservationCommand is sent to the Inventory service with the OrderId
- **AND** an OrderRejected event is published with reason "Stock reservation timeout"

#### Scenario: Release command is sent even if reservation status is unknown
- **WHEN** the timeout fires and it is unknown whether the Inventory service created a reservation
- **THEN** the ReleaseReservationCommand is sent anyway
- **AND** the Inventory service handles idempotently (no-op if no reservation exists for OrderId)

---

### Requirement: Saga triggers compensation when order is cancelled after stock reserved
The system SHALL trigger compensation when an OrderCancelled command is received while the saga is in the `StockReserved` state. The compensation SHALL release the existing stock reservation and publish an OrderCancelled integration event.

#### Scenario: Order cancelled after stock reserved releases reservation
- **WHEN** an OrderCancelled command is received while saga is in `StockReserved` state
- **THEN** the saga transitions to `Compensating`
- **AND** a ReleaseReservationCommand is sent using the stored ReservationId
- **AND** an OrderCancelled integration event is published

---

### Requirement: ReleaseReservationCommand is idempotent
The Inventory service SHALL handle ReleaseReservationCommand idempotently. If no reservation exists for the given OrderId or the reservation is already in Released status, the operation SHALL succeed without error and without modifying any stock values. This ensures safe compensation regardless of whether the initial reservation completed.

#### Scenario: Release command on non-existent reservation is a no-op
- **WHEN** a ReleaseReservationCommand is received for an OrderId with no existing reservation
- **THEN** the operation succeeds
- **AND** no stock values are modified in Postgres or Redis

#### Scenario: Release command on already-released reservation is a no-op
- **WHEN** a ReleaseReservationCommand is received for a reservation with Status = Released
- **THEN** the operation succeeds
- **AND** no stock values are modified

#### Scenario: Release command on Pending reservation releases stock
- **WHEN** a ReleaseReservationCommand is received for a reservation with Status = Pending
- **THEN** StockAvailable is incremented and ReservedStock is decremented in Postgres
- **AND** Redis is updated via compensation Lua script
- **AND** the reservation Status is set to Released and ReleasedAt is set to UtcNow

---

### Requirement: Compensation preserves an audit trail of all compensating actions
The system SHALL record all compensation events in a structured format. Every ReleaseReservationCommand execution SHALL update the StockReservation record with a Released status and ReleasedAt timestamp. Every OrderRejected and OrderCancelled event SHALL be observable via the integration event bus.

#### Scenario: Released reservation has audit timestamps
- **WHEN** a reservation is released via compensation
- **THEN** the StockReservation record has Status = Released and ReleasedAt = UtcNow
- **AND** the InventoryId, OrderId, Quantity, and ReservedAt fields remain unchanged

#### Scenario: Order rejection is published as an observable integration event
- **WHEN** an OrderRejected event is published as part of compensation
- **THEN** the event contains OrderId, Reason, and Timestamp
- **AND** the event is visible on the integration event bus for downstream consumers (e.g., notifications)
