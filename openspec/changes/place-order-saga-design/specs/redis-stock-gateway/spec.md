## ADDED Requirements

### Requirement: Redis maintains two counters per variant for available and reserved stock
The system SHALL maintain two Redis string keys per product variant:
- `stock:available:{variantId}` — current available stock (can be reserved)
- `stock:reserved:{variantId}` — stock tentatively reserved pending Postgres confirmation

Both keys SHALL be initialized from Postgres values on service startup. All modifications to these keys SHALL occur only via Lua scripts to guarantee atomicity.

#### Scenario: Redis keys are present for each inventory variant
- **WHEN** the Inventory service starts
- **THEN** `stock:available:{variantId}` and `stock:reserved:{variantId}` exist in Redis for every active inventory variant
- **AND** their values equal the corresponding Postgres `StockAvailable` and `ReservedStock` columns

#### Scenario: Direct key manipulation outside Lua scripts is forbidden
- **WHEN** application code attempts to call INCRBY or DECRBY on stock keys outside a Lua script
- **THEN** this SHALL be considered a defect — all stock key modifications MUST go through the designated Lua scripts

---

### Requirement: Check-and-reserve Lua script is atomic and all-or-nothing across all items
The system SHALL execute a single Lua script to check availability and reserve stock for all items in an order. The script SHALL first verify that ALL items have sufficient `stock:available` before modifying ANY keys. If any item fails the check, no keys SHALL be modified. The script SHALL execute as an atomic unit — no other Redis commands can interleave during execution.

#### Scenario: All items pass availability check — all reserved atomically
- **WHEN** the check-and-reserve script is called with N items and all have sufficient available stock
- **THEN** all N `stock:available` keys are decremented by their requested quantities
- **AND** all N `stock:reserved` keys are incremented by their requested quantities
- **AND** the script returns `{1, "success"}`

#### Scenario: One item fails availability check — no keys modified
- **WHEN** the check-and-reserve script is called and at least one item has insufficient stock
- **THEN** no `stock:available` or `stock:reserved` keys are modified for any item
- **AND** the script returns `{0, "<variantId>"}` identifying the first failing variant

#### Scenario: Script completes under concurrent load without partial state
- **WHEN** 1000 concurrent requests invoke the check-and-reserve Lua script on the same variant
- **THEN** each request either fully succeeds or fully fails
- **AND** no request observes a partially modified stock state

---

### Requirement: Compensation Lua script atomically restores stock on Postgres failure
The system SHALL execute a Lua compensation script to restore tentatively reserved stock back to available when the Postgres transaction fails or rolls back. The compensation script SHALL increment `stock:available` and decrement `stock:reserved` for all items in the failed reservation atomically.

#### Scenario: Postgres transaction rollback triggers Redis compensation
- **WHEN** a Postgres transaction fails after the check-and-reserve Lua script succeeded
- **THEN** the compensation Lua script is invoked with the same items and quantities
- **AND** all `stock:available` keys are incremented by their reserved quantities
- **AND** all `stock:reserved` keys are decremented by their reserved quantities

#### Scenario: Compensation script is idempotent for retry safety
- **WHEN** the compensation script is called twice for the same items due to a retry
- **THEN** the second invocation increments available and decrements reserved again
- **AND** the application MUST NOT call compensation twice for the same reservation — idempotency is enforced at the application layer via the idempotency check in `inventory-reservation`

---

### Requirement: Redis cache is rebuilt from Postgres on service startup
The system SHALL rebuild the Redis stock cache from Postgres on every Inventory service startup if the cache is not populated. The rebuild SHALL load all Inventory records and set both `stock:available` and `stock:reserved` keys. A sentinel key (`stock:_initialized`) SHALL be used to detect an already-populated cache and avoid redundant rebuilds.

#### Scenario: Cold start with empty Redis populates cache from Postgres
- **WHEN** the Inventory service starts and `stock:_initialized` key does not exist in Redis
- **THEN** all inventory variants are loaded from Postgres
- **AND** `stock:available:{variantId}` and `stock:reserved:{variantId}` are set for each variant
- **AND** `stock:_initialized` is set to mark cache as ready

#### Scenario: Warm start with existing Redis skips rebuild
- **WHEN** the Inventory service starts and `stock:_initialized` exists in Redis
- **THEN** no Postgres read is performed for cache initialization
- **AND** the existing Redis values are used as-is

---

### Requirement: Background sync reconciles Redis with Postgres every 5 minutes
The system SHALL run a Hangfire recurring job every 5 minutes that reads all Inventory records from Postgres and updates the corresponding Redis keys. If any Redis value diverges from Postgres by more than a configurable threshold (default: 10 units), the system SHALL log a warning and overwrite the Redis value with the Postgres value.

#### Scenario: Periodic sync corrects Redis divergence
- **WHEN** the sync job runs and finds a Redis value that differs from Postgres by more than the threshold
- **THEN** the Redis key is overwritten with the Postgres value
- **AND** a warning is logged with the VariantId, Redis value, and Postgres value

#### Scenario: Periodic sync on consistent data makes no changes
- **WHEN** the sync job runs and all Redis values match Postgres within the threshold
- **THEN** no Redis keys are modified
- **AND** no warnings are logged

---

### Requirement: Redis unavailability degrades gracefully without blocking orders
The system SHALL detect Redis connectivity failures and fall back to Postgres-only stock verification when Redis is unavailable. In fallback mode, the Redis fast-gate step is skipped and stock verification proceeds directly via Postgres with a pessimistic lock (SELECT FOR UPDATE). A circuit breaker SHALL prevent repeated failed Redis connection attempts.

#### Scenario: Redis unavailable triggers fallback to Postgres-only path
- **WHEN** a ReserveStockCommand is received and Redis connection fails
- **THEN** the system bypasses the Redis fast-gate
- **AND** proceeds directly to Postgres stock verification and reservation
- **AND** a warning metric is emitted indicating Redis fallback mode

#### Scenario: Circuit breaker prevents repeated Redis connection attempts
- **WHEN** Redis has been unavailable for more than 30 seconds
- **THEN** the circuit breaker opens and Redis connection is not attempted for subsequent requests
- **AND** all requests operate in Postgres-only fallback mode until Redis recovers
