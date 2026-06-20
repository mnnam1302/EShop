## Context

### Current State

The existing PlaceOrderCommandHandler (V1) implements a basic flow:
- Accepts PlaceOrderCommand from API
- Creates Order aggregate and persists to Order database
- No inventory verification before order creation
- No distributed transaction coordination
- No compensation logic for failures

This leads to critical production issues:
- **Overselling**: Orders accepted without checking stock availability
- **Race conditions**: High concurrency causes lost updates on stock deduction
- **No recovery**: Failed operations leave system in inconsistent state
- **Poor observability**: No visibility into transaction progress

### Constraints

- Must support **10K+ concurrent order requests**
- Must maintain **data consistency** across Order and Inventory services
- Must be **idempotent** (handle duplicate requests safely)
- Must integrate with existing **MassTransit + RabbitMQ** event bus
- Must use existing **PostgreSQL** for persistence and **Redis** for caching
- Must follow existing **.NET CQRS patterns** (ICommand/ICommandHandler)
- Future extensibility for Finance service (payment processing)

### Stakeholders

- Order service team: Saga orchestrator implementation
- Inventory service: Stock reservation commands and compensation
- Infrastructure team: Redis deployment, MassTransit saga persistence
- QA: High-concurrency testing, failure scenario validation

---

## Goals / Non-Goals

### Goals

1. **Distributed Transaction Coordination**: Orchestrate Place Order workflow across Order and Inventory services with proper state management
2. **High-Concurrency Stock Management**: Handle 10K+ concurrent requests with Redis fast-path and atomic operations
3. **Idempotency Guarantees**: Safely handle duplicate requests and retries at all levels (saga, command, database)
4. **Data Consistency**: Maintain Redis-Postgres consistency with authoritative source of truth pattern
5. **Observability**: Track saga state and transaction progress for debugging and monitoring
6. **Compensation Logic**: Automatic rollback of partial transactions on failure
7. **Future-Ready**: Extensible design for adding Finance service payment processing

### Non-Goals

- ❌ **Choreography pattern**: V1 uses orchestration; choreography deferred to V2
- ❌ **Multi-region consistency**: Single-region deployment for V1
- ❌ **Event sourcing**: Using traditional state-based saga persistence
- ❌ **2PC (Two-Phase Commit)**: Using saga pattern with compensating transactions instead
- ❌ **Inventory forecasting**: Only checking current available stock, no predictive analytics
- ❌ **Payment processing**: Finance service integration is future work (V2)

---

## Decisions

### Decision 1: Orchestration Pattern for V1

**Choice**: MassTransit Automatonymous Saga (Orchestration)

**Rationale**:
- **Clear workflow visibility**: State machine defines entire process in one place
- **Built-in compensation**: Framework handles timeouts, retries, and compensation ordering
- **Single state source**: Query saga state for transaction progress (better observability)
- **Easier testing**: Test workflow logic independently from service implementations
- **Future extensibility**: Adding payment step only requires saga state machine changes

**Alternatives Considered**:
- **Choreography** (event-driven, decentralized):
  - ✅ Pros: Loose coupling, natural scalability, existing pattern in codebase
  - ❌ Cons: Distributed state, implicit workflow, complex compensation coordination
  - 📋 Decision: Defer to V2 after orchestration proves successful

**Trade-offs**:
- Central orchestrator is potential bottleneck (mitigated by horizontal scaling)
- Saga state persistence adds infrastructure complexity
- Team learning curve for MassTransit state machines

---

### Decision 2: Redis-Postgres Consistency Model

**Choice**: Eventual consistency with Postgres as authoritative source

**Architecture**:
```
┌─────────────────────────────────────────┐
│  Redis: Performance Cache (Fast Path)  │
│  - Fast rejection of insufficient stock │
│  - Eventually consistent                │
│  - Rebuild from Postgres on crash       │
└──────────────┬──────────────────────────┘
               ▼
┌─────────────────────────────────────────┐
│  Postgres: Source of Truth              │
│  - Final decision on stock availability │
│  - Strongly consistent (ACID)           │
│  - Durable, survives crashes            │
└─────────────────────────────────────────┘
```

**Flow**:
1. **Redis fast-gate** (Lua script): Check and tentatively reserve stock
2. **Postgres verification** (transaction): Authoritative check and persistent reservation
3. **Compensation on conflict**: If Postgres rejects, rollback Redis reservation
4. **Background sync**: Reconcile Redis with Postgres every 5 minutes
5. **Startup rebuild**: Reload Redis cache from Postgres on service start

**Rationale**:
- Redis provides sub-millisecond rejection for insufficient stock (99% of conflicts)
- Postgres prevents overselling even if Redis is stale (safety net)
- Acceptable for Redis to be slightly stale (fast-path optimization, not guarantee)
- Background sync prevents unbounded divergence

**Alternatives Considered**:
- **Redis as source of truth**: Too risky (volatile, data loss on crash)
- **Postgres only**: Cannot handle 10K+ concurrent reads (too slow)
- **Distributed locks**: Poor concurrency, deadlock risk, timeout complexity

---

### Decision 3: Three-Level Idempotency Strategy

**Level 1: Saga Instance Deduplication**
- Use OrderId as saga CorrelationId
- MassTransit prevents duplicate saga creation for same CorrelationId
- Duplicate OrderSubmitted events find existing saga instance

**Level 2: Command Deduplication (IdempotencyKey)**
- Saga passes CorrelationId as IdempotencyKey to commands
- Services check: "Did I already process this IdempotencyKey?"
- Return existing result for duplicate commands (idempotent response)

**Level 3: Database Constraint Enforcement**
- UNIQUE INDEX on `StockReservations.IdempotencyKey`
- Prevents duplicate inserts even if application-level checks missed
- Database enforces exactly-once semantics

**Implementation**:
```sql
CREATE TABLE "StockReservations" (
    "Id" uuid PRIMARY KEY,
    "IdempotencyKey" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "Status" varchar(20) NOT NULL,
    ...
);

CREATE UNIQUE INDEX "IX_StockReservations_IdempotencyKey" 
    ON "StockReservations"("IdempotencyKey") 
    WHERE "Status" != 'Released';
```

**Rationale**:
- Defense-in-depth: Multiple layers prevent duplicate processing
- Saga-level prevents duplicate workflows
- Service-level prevents duplicate operations
- Database-level is final safety net

---

### Decision 4: Atomic Operations via Redis Lua + Postgres Transactions

**Redis Lua Script** (atomic multi-item check-and-reserve):
```lua
-- Phase 1: Check all items have sufficient stock
for i = 1, #KEYS do
    local available = redis.call('GET', 'stock:available:' .. KEYS[i])
    if tonumber(available) < tonumber(ARGV[i]) then
        return { 0, KEYS[i] }  -- Failed, return failed variantId
    end
end

-- Phase 2: All checks passed - atomically reserve all items
for i = 1, #KEYS do
    redis.call('DECRBY', 'stock:available:' .. KEYS[i], ARGV[i])
    redis.call('INCRBY', 'stock:reserved:' .. KEYS[i], ARGV[i])
end

return { 1, 'success' }
```

**Atomicity Guarantees**:
- Redis: Single-threaded Lua execution (no interleaving)
- Postgres: BEGIN/COMMIT transaction (all-or-nothing)
- Compensation: Rollback Redis if Postgres fails

**Rationale**:
- Lua script ensures multi-item reservation is atomic (no partial reservations)
- Postgres transaction ensures inventory update + reservation record inserted together
- Compensating Lua script returns stock if transaction fails

---

### Decision 5: Saga State Machine Design

**States**:
- `Initial` → Order submitted, saga starts
- `StockChecking` → Waiting for inventory reservation response
- `StockReserved` → Stock reserved, persisting order
- `Completed` → Order persisted, saga finished
- `Failed` → Stock reservation failed, order rejected
- `Compensating` → Rolling back partial operations

**Key Features**:
- **Timeout handling**: 30-second timeout per step (prevents hung sagas)
- **Automatic retries**: Exponential backoff on transient failures
- **Compensation ordering**: Release reservations in reverse order
- **State persistence**: PostgreSQL saga state table for recovery

**Saga State Schema**:
```sql
CREATE TABLE "SagaStates" (
    "CorrelationId" uuid PRIMARY KEY,
    "CurrentState" varchar(50) NOT NULL,
    "OrderId" uuid NOT NULL,
    "Items" jsonb NOT NULL,
    "ReservationId" uuid,
    "SubmittedAt" timestamptz NOT NULL,
    "CompletedAt" timestamptz,
    "FailureReason" text
);
```

**Rationale**:
- Explicit state machine makes workflow visible and testable
- Timeouts prevent saga instances from hanging indefinitely
- Persisted state enables saga recovery after service restart
- Correlation by OrderId enables idempotent saga creation

---

### Decision 6: Reservation Expiration Strategy

**Choice**: 15-minute reservation timeout with background expiration job

**Flow**:
1. Stock reserved with `ExpiresAt = UtcNow + 15 minutes`
2. Hangfire background job runs every 1 minute
3. Job finds expired reservations: `WHERE Status='Pending' AND ExpiresAt < UtcNow`
4. Release stock: `Available++, Reserved--`
5. Update Redis to reflect released stock
6. Publish `ReservationExpired` event for saga compensation

**Rationale**:
- Prevents indefinite stock locks from abandoned carts or crashed sagas
- 15 minutes balances user checkout time vs. stock availability
- Background job ensures eventual consistency even if saga fails to compensate
- Redis sync after expiration prevents stale cache

**Alternatives Considered**:
- **No expiration**: Risk of permanently locked stock
- **Shorter timeout** (5 min): Too aggressive, poor UX for legitimate slow checkouts
- **Longer timeout** (30 min): Stock unavailable too long, poor inventory turnover

---

### Decision 7: Optimistic Concurrency in Postgres

**Choice**: Row version checking with retry policy

**Implementation**:
```csharp
public class Inventory : AggregateRoot<Guid>
{
    public int StockAvailable { get; set; }
    public byte[] RowVersion { get; set; }  // Concurrency token
}

// EF Core configuration
builder.Property(e => e.RowVersion)
    .IsRowVersion()
    .IsConcurrencyToken();
```

**Retry Policy**:
- Exponential backoff: 1s, 2s, 4s, 8s, 16s
- Max 5 retries
- Fail saga after exhausting retries

**Rationale**:
- Optimistic concurrency has better throughput than pessimistic locks (SELECT FOR UPDATE)
- Row version automatically managed by EF Core
- Retry policy handles transient conflicts from concurrent reservations
- MassTransit automatically retries on `DbUpdateConcurrencyException`

**Trade-offs**:
- High contention scenarios may exhaust retries (acceptable for rare popular items)
- Retries add latency (mitigated by exponential backoff)

---

## Risks / Trade-offs

### Risk 1: Redis Cache Staleness → Overselling

**Risk**: Redis cache is stale (shows stock available), but Postgres has no stock. User sees "in stock" but order is rejected.

**Mitigation**:
- Postgres is final authority (always checked before commit)
- Background sync every 5 minutes reduces staleness window
- Monitoring alerts on Redis-Postgres divergence > 10 units
- Acceptable trade-off: Better to show availability optimistically and reject occasionally than slow down all requests

---

### Risk 2: Saga State Table Contention

**Risk**: High concurrent order volume causes contention on saga state table writes (10K+ concurrent saga instances).

**Mitigation**:
- Use optimistic concurrency on saga state (MassTransit default)
- Partition saga state table by CorrelationId hash (future optimization)
- Horizontal scaling of saga host (multiple instances processing sagas)
- Monitoring: Track saga state persistence latency, alert on P99 > 500ms

---

### Risk 3: Redis Crash → Performance Degradation

**Risk**: Redis crash forces all requests to hit Postgres directly (much slower).

**Mitigation**:
- Graceful degradation: System still functions, just slower
- Auto-rebuild Redis cache on startup (from Postgres)
- Redis HA deployment (Redis Sentinel or Cluster) for production
- Circuit breaker: Detect Redis unavailability and bypass fast-path
- Monitoring: Alert on Redis unavailability, track fallback request rate

---

### Risk 4: Saga Timeout → Stock Locked

**Risk**: Saga times out after reserving stock but before completing. Stock remains reserved until expiration job runs.

**Mitigation**:
- Short saga timeout (30 seconds per step)
- Background expiration job runs every 1 minute (max 1-minute lock time)
- Saga compensation on timeout (attempts to release immediately)
- Monitoring: Track saga timeout rate, investigate if > 1%

---

### Risk 5: Duplicate Message Processing

**Risk**: Network issues cause duplicate messages (OrderSubmitted, ReserveStock) delivered multiple times.

**Mitigation**:
- Three-level idempotency (saga, command, database constraint)
- MassTransit inbox pattern (message deduplication at consumer level)
- IdempotencyKey uniqueness enforced by database
- Idempotent handlers return existing result for duplicates
- All operations designed for safe retries

---

### Risk 6: Inventory Service Downtime → All Orders Fail

**Risk**: Inventory service unavailable (deployment, crash), all order placements fail.

**Mitigation**:
- Saga retry policy (exponential backoff, 5 retries over ~30 seconds)
- Circuit breaker: Detect inventory service outage, fail fast with clear error
- Graceful degradation: Accept orders in "pending verification" state (manual review)
- Health checks: Proactive alerting before service becomes unavailable
- Blue-green deployment strategy to minimize downtime

---

## Migration Plan

### Phase 1: Infrastructure Setup

**Prerequisites**:
1. Deploy Redis instance (development: single node, production: Redis Sentinel HA)
2. Apply database migrations:
   - Create `SagaStates` table
   - Create `StockReservations` table
   - Add `RowVersion` column to `Inventories`
3. Configure MassTransit saga persistence (Entity Framework repository)
4. Deploy Hangfire for background jobs (expiration, sync)

**Validation**:
- Redis connectivity test
- Saga state CRUD operations
- Background jobs execute on schedule

---

### Phase 2: Feature Flag Rollout

**Strategy**: Gradual rollout with feature flag

```csharp
public class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    private readonly IFeatureManager _featureManager;
    
    public async Task<Result> HandleAsync(PlaceOrderCommand command, CancellationToken ct)
    {
        if (await _featureManager.IsEnabledAsync("UseSagaOrchestration"))
        {
            // V1: Saga orchestration path
            return await _sagaOrchestrator.InitiatePlaceOrderSaga(command, ct);
        }
        else
        {
            // V0: Legacy direct persistence path
            return await _legacyHandler.HandleAsync(command, ct);
        }
    }
}
```

**Rollout Steps**:
1. Week 1: 1% of traffic → Monitor saga success rate, Redis performance
2. Week 2: 10% of traffic → Validate idempotency, compensation logic
3. Week 3: 50% of traffic → Stress test high concurrency (10K+ requests)
4. Week 4: 100% of traffic → Full production rollout
5. Week 6: Remove feature flag and legacy code path

**Rollback Strategy**:
- Feature flag OFF → Instant rollback to legacy behavior
- No data migration needed (saga state is additive)
- Monitor: Order success rate, inventory consistency, system latency

---

### Phase 3: Monitoring & Alerting

**Key Metrics**:
- Saga duration (P50, P95, P99): Alert if P99 > 5 seconds
- Saga success rate: Alert if < 99%
- Saga timeout rate: Alert if > 1%
- Redis hit rate: Alert if < 95%
- Redis-Postgres divergence: Alert if any variant > 10 units difference
- Reservation expiration rate: Alert if > 5% of reservations expire

**Dashboards**:
- Saga state distribution (% in each state)
- Stock reservation funnel (submitted → reserved → completed → failed)
- Redis vs Postgres stock level comparison
- Background job execution health

---

## Open Questions

### Q1: Redis Deployment Strategy

**Question**: Single Redis instance or Redis Cluster for production?

**Options**:
- Single instance with Redis Sentinel (HA, automatic failover)
- Redis Cluster (horizontal scaling, data partitioning)

**Decision Needed**: Depends on expected read volume and availability SLA. Recommend Sentinel for V1 (simpler), evaluate Cluster if throughput insufficient.

---

### Q2: Saga Timeout Values

**Question**: What are the appropriate timeout values per step?

**Current Proposal**:
- Stock reservation: 30 seconds
- Order persistence: 10 seconds
- Overall saga: 2 minutes

**Validation Needed**: Load testing to determine realistic P99 latencies under high concurrency.

---

### Q3: Inventory Multi-Warehouse Support

**Question**: Does inventory reservation need to consider multiple warehouses?

**Impact**: Lua script complexity, reservation strategy (reserve from closest warehouse first?)

**Current Assumption**: Single logical inventory per variant (warehouse routing is future work).

---

### Q4: Partial Order Fulfillment

**Question**: If order has 3 items, but only 2 available, accept partial order or reject entirely?

**Current Design**: Reject entire order (all-or-nothing). Partial fulfillment requires UX changes and additional saga states.

**Decision Needed**: Product team input on business requirements.

---

### Q5: Payment Processing Integration Point

**Question**: When to integrate Finance service? Before or after order persistence?

**Options**:
- **Before persistence**: Reserve stock → Process payment → Persist order (current design)
- **After persistence**: Persist order → Process payment → Confirm order

**Trade-off**: Before persistence prevents unpaid orders in database, but payment timeout risks stock lock. After persistence allows async payment processing.

**Decision Needed**: Finance team input on payment gateway constraints and expected latency.
