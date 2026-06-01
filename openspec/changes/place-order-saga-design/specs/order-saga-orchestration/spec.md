## ADDED Requirements

### Requirement: Saga instance created per order submission
The system SHALL create exactly one saga instance per OrderId when a PlaceOrderCommand is received. The saga CorrelationId SHALL equal the OrderId to enable idempotent creation. If a saga instance already exists for the given CorrelationId, the system SHALL find the existing instance and NOT create a duplicate.

#### Scenario: New order submitted creates saga instance
- **WHEN** a PlaceOrderCommand is received with a unique OrderId
- **THEN** a new PlaceOrderSaga instance is created with CorrelationId = OrderId
- **AND** the saga transitions to `StockChecking` state
- **AND** a ReserveStockCommand is sent to the Inventory service with IdempotencyKey = CorrelationId
- **AND** the saga state is persisted to the SagaStates table

#### Scenario: Duplicate order submission does not create second saga
- **WHEN** a PlaceOrderCommand is received with an OrderId that already has an active saga
- **THEN** the existing saga instance is found
- **AND** no second saga instance is created
- **AND** no duplicate ReserveStockCommand is sent

---

### Requirement: Saga coordinates order flow through well-defined states
The saga SHALL progress through the following states in sequence: `Initial` → `StockChecking` → `StockReserved` → `Completed`. The saga SHALL transition to `Failed` if stock reservation fails. The saga SHALL transition to `Compensating` if a timeout occurs or cancellation is requested while in an intermediate state.

#### Scenario: Successful order placement state flow
- **WHEN** a saga is created and stock is successfully reserved
- **THEN** saga transitions: Initial → StockChecking → StockReserved
- **AND** a PersistOrderCommand is sent to the Order service
- **WHEN** the order is persisted
- **THEN** saga transitions to Completed and is finalized

#### Scenario: Stock reservation failure transitions to Failed
- **WHEN** the Inventory service responds with StockReservationFailed
- **THEN** the saga transitions from `StockChecking` to `Failed`
- **AND** an OrderRejected integration event is published with the failure reason
- **AND** no PersistOrderCommand is sent

#### Scenario: Compensating state entered on mid-flow cancellation
- **WHEN** an OrderCancelled command is received while the saga is in `StockReserved` state
- **THEN** the saga transitions to `Compensating`
- **AND** a ReleaseReservationCommand is sent to the Inventory service

---

### Requirement: Saga enforces per-step timeouts to prevent hanging instances
The system SHALL apply a configurable timeout per saga step. If the timeout expires before a response is received, the saga SHALL transition to `Compensating`, trigger compensation for any completed steps, and publish an OrderRejected event.

#### Scenario: Stock reservation timeout triggers compensation
- **WHEN** a ReserveStockCommand is sent and no response is received within 30 seconds
- **THEN** the saga transitions to `Compensating`
- **AND** a ReleaseReservationCommand is sent to release any partial reservation
- **AND** an OrderRejected event is published with reason "Stock reservation timeout"

#### Scenario: Timeout token is cancelled on successful response
- **WHEN** the Inventory service responds with StockReserved before the timeout elapses
- **THEN** the pending timeout is unscheduled
- **AND** no spurious compensation is triggered

---

### Requirement: Saga state is persisted to PostgreSQL for crash recovery
The system SHALL persist saga state to a `SagaStates` PostgreSQL table after every state transition. On service restart, the system SHALL resume in-progress sagas from their last persisted state. Saga persistence SHALL use optimistic concurrency to prevent concurrent saga processing conflicts.

#### Scenario: Saga state recovered after service restart
- **WHEN** the Order service restarts while a saga is in `StockChecking` state
- **THEN** the saga resumes from `StockChecking` state on restart
- **AND** the pending timeout is re-evaluated
- **AND** if the timeout has already elapsed, the saga transitions to `Compensating`

#### Scenario: Concurrent saga processing conflict is handled
- **WHEN** two service instances attempt to process the same saga simultaneously
- **THEN** optimistic concurrency detection prevents conflicting updates
- **AND** only one instance proceeds and the other retries

---

### Requirement: Saga applies retry policy on transient failures
The system SHALL retry saga step commands using exponential backoff when transient failures occur (network errors, service unavailability). Retries SHALL NOT be applied to business failures (insufficient stock, invalid order).

#### Scenario: Transient failure on ReserveStockCommand triggers retry
- **WHEN** a ReserveStockCommand delivery fails due to a transient RabbitMQ error
- **THEN** the system retries delivery with exponential backoff: 1s, 2s, 4s, 8s, 16s
- **AND** after 5 failed retries the saga transitions to `Failed`

#### Scenario: Business failure is not retried
- **WHEN** the Inventory service returns StockReservationFailed with reason "Insufficient stock"
- **THEN** no retry is attempted
- **AND** the saga immediately transitions to `Failed`

---

### Requirement: Completed and Failed sagas are archived and removed from active state table
The system SHALL finalize and remove completed or permanently failed sagas from the `SagaStates` table to prevent unbounded table growth. Finalized sagas SHALL be archived to an `ArchivedSagaStates` table or equivalent for audit purposes.

#### Scenario: Completed saga is finalized
- **WHEN** the saga transitions to `Completed`
- **THEN** the saga instance is marked finalized and removed from the active SagaStates table

#### Scenario: Permanently failed saga is finalized
- **WHEN** the saga transitions to `Failed` with no further retries
- **THEN** the saga is finalized and removed from the active SagaStates table
