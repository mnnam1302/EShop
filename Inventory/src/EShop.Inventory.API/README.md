# Inventory Service

> Single source of truth for **stock levels**. Deducts stock atomically at order placement. Releases stock on cancel, payment fail, or TTL expiry.

---

## What This Service Does

```mermaid
graph LR
    classDef event   fill:#ff9800,color:#fff,stroke:none
    classDef command fill:#42a5f5,color:#fff,stroke:none
    classDef agg     fill:#f48fb1,color:#000,stroke:#c62828
    classDef actor   fill:#fff9c4,color:#000,stroke:#f9a825

    CAT([Catalog]):::actor
    ORD([Order Service]):::actor
    INV(Inventory Service):::agg

    CAT -->|"VariantCreated / ProductDeleted"| INV
    ORD -->|"MakeReservation"| INV
    ORD -->|"ReleaseReservationCommand"| INV
    ORD -->|"ConfirmReservationCommand"| INV
    INV -->|"StocksReserved"| ORD
    INV -->|"StocksNotReserved"| ORD
```

---

## Core Concept — Deduct on Order

> Stock is **permanently removed** from `StockAvailable` the moment an order is placed — not at payment, not at shipping.

```mermaid
flowchart LR
    classDef command fill:#42a5f5,color:#fff,stroke:none
    classDef event   fill:#ff9800,color:#fff,stroke:none
    classDef agg     fill:#f48fb1,color:#000,stroke:#c62828

    A[100 units available]:::agg
    B[97 units available<br/>Hold record created]:::agg
    C([Hold = Confirmed<br/>97 units stay]):::event
    D([Hold = Released<br/>100 units restored]):::event

    A -->|"MakeReservation — order placed for 3"| B
    B -->|"ConfirmReservationCommand — payment OK"| C
    B -->|"ReleaseReservationCommand — cancel / fail / timeout"| D
```

The **release path is mandatory** — every failed order must add stock back.

---

## Domain Model

```mermaid
classDiagram
    class Inventory {
        Guid VariantId
        int StockAvailable
        int ReservedStock
        int MinimumStock
    }

    class Reservation {
        Guid OrderId
        string Status
        DateTimeOffset ExpiresAt
        +Create()
        +Confirm()
        +Release()
        +Expire()
    }

    class ReservationItem {
        Guid VariantId
        int Quantity
    }

    Reservation "1" --> "1..*" ReservationItem : Items
    Inventory "1" ..> "0..*" Reservation : tracked via VariantId
```

| | `Inventory` | `Reservation` | `ReservationItem` |
|-|------------|--------------|------------------|
| One per | SKU x tenant | Order | Variant x order |
| Key constraint | — | `UNIQUE(tenant_id, order_id)` | `UNIQUE(reservation_id, variant_id)` |
| Purpose | Holds stock counts | Tracks the hold status + TTL | Stores qty per variant for add-back |

---

## Stock Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending : MakeReservation<br/>CAS deduct + Hold created

    Pending --> Confirmed : ConfirmReservationCommand<br/>payment success — no stock change

    Pending --> Released : ReleaseReservationCommand<br/>cancel / payment fail<br/>available += qty

    Pending --> Expired  : TTL sweeper — 15 min unpaid<br/>available += qty

    Confirmed --> [*]
    Released  --> [*]
    Expired   --> [*]
```

---

## Architecture — Three Layers

```mermaid
flowchart TB
    classDef command fill:#42a5f5,color:#fff,stroke:none
    classDef event   fill:#ff9800,color:#fff,stroke:none

    REQ[MakeReservation]:::command

    subgraph L1["Layer 1 — Fast Gate"]
        R["Redis Lua atomic script"]
    end
    subgraph L2["Layer 2 — System of Record"]
        PG["PostgreSQL CAS UPDATE"]
    end
    subgraph L3["Layer 3 — Reliability"]
        OB["Outbox — RabbitMQ relay"]
    end

    FAIL([StocksNotReserved]):::event

    REQ --> L1
    L1 -->|"gate passed"| L2
    L2 -->|"rows = 1 committed"| L3
    L1 -->|"gate rejected"| FAIL
    L2 -->|"rows = 0 — rollback"| FAIL
```

| Layer | Role | Source of Truth? |
|-------|------|-----------------|
| Redis | Reject sold-out in under 1 ms | No — cache only |
| PostgreSQL CAS | Final no-oversell decision | Yes |
| Outbox | Publish event after commit | — |

---

## Happy Path — Step by Step

```mermaid
sequenceDiagram
    autonumber
    participant PM   as Process Manager
    participant MC   as MakeReservationConsumer
    participant H    as ReserveStocksCommandHandler
    participant RD   as Redis
    participant PG   as PostgreSQL
    participant OB   as OutboxRelay

    PM->>MC: MakeReservation(OrderId, Items[])
    MC->>H: ReserveStocksCommand (inbox dedup check)
    Note over H: Sort items by VariantId asc — deadlock prevention
    H->>RD: TryReserveAsync — Lua gate, all-or-none
    RD-->>H: OK

    rect rgb(225,245,254)
        Note over H,PG: One transaction — everything or nothing
        H->>PG: CAS UPDATE available -= qty (per item, sorted)
        H->>PG: INSERT Reservation (Pending, +15 min)
        H->>PG: INSERT ReservationItem x N
        H->>PG: INSERT OutboxMessage (StocksReserved)
        H->>PG: COMMIT
    end

    OB->>PM: StocksReserved(OrderId, ReservationId)
```

**Why one transaction?** If the app crashes after deducting stock but before writing the outbox, the whole transaction rolls back. No orphaned deductions, no lost events.

---

## Release Paths

```mermaid
flowchart TD
    classDef command fill:#42a5f5,color:#fff,stroke:none
    classDef pm      fill:#ce93d8,color:#000,stroke:#7b1fa2

    T1[ReleaseReservationCommand<br/>saga compensation]:::command
    T2[ExpireReservationsJob<br/>Hangfire every 1 min]
    T3[ConfirmReservationCommand<br/>payment success]:::command

    GUARD{"Status == Pending?"}
    SKIP[No-op — already terminal]
    ADD[available += qty per item<br/>commit]
    REDIS[Redis ReleaseAsync<br/>compensate counters]
    MARK[Mark Released / Expired / Confirmed]

    T1 & T2 --> GUARD
    T3 --> MARK
    GUARD -->|"No"| SKIP
    GUARD -->|"Yes"| ADD --> REDIS --> MARK
```

The **status guard** means whichever trigger fires first wins — the others are no-ops.

---

## Concurrency Rules

```mermaid
flowchart LR
    subgraph CAS["CAS — different orders racing"]
        C1["Order A wants 1 unit"]
        C2["Order B wants 1 unit"]
        DB[("1 unit left")]
        W["One gets rows=1<br/>Other gets rows=0"]
        C1 & C2 -->|"WHERE available >= 1"| DB --> W
    end

    subgraph IDEM["Idempotency — same order redelivered"]
        I1["MakeReservation delivery 1"]
        I2["MakeReservation delivery 2 retry"]
        INBOX["Inbox check<br/>+ UNIQUE(order_id)"]
        ONE["Only one deducts"]
        I1 & I2 --> INBOX --> ONE
    end
```

| Problem | Mechanism | Guard |
|---------|-----------|-------|
| Two buyers, last unit | CAS `WHERE available >= qty` | Database |
| Redelivered message | Inbox + `UNIQUE(tenant_id, order_id)` | Same transaction as CAS |
| Deadlock [A,B] vs [B,A] | Sort by `VariantId` asc | Application |

---

## Background Jobs

```mermaid
gantt
    title Background Jobs Schedule
    dateFormat s
    axisFormat %S s

    section Startup
    RedisStockInitializer (once)  : 0, 1s

    section Recurring
    RelayOutboxJob every 5 s      : 0, 5s
    ExpireReservationsJob 1 min   : 0, 60s
    SyncRedisStockJob 5 min       : 0, 300s
```

| Job | What it does |
|-----|-------------|
| `RedisStockInitializer` | Seeds Redis counters from Postgres on startup |
| `RelayOutboxJob` | Publishes pending outbox rows to RabbitMQ (batch 50, SKIP LOCKED) |
| `ExpireReservationsJob` | Finds `Pending` reservations past `ExpiresAt` — add-back + `Expired` |
| `SyncRedisStockJob` | Re-seeds all Redis counters from Postgres — heals any drift |

---

## Integration Events

```mermaid
graph LR
    classDef command fill:#42a5f5,color:#fff,stroke:none
    classDef event   fill:#ff9800,color:#fff,stroke:none
    classDef agg     fill:#f48fb1,color:#000,stroke:#c62828

    MR[MakeReservation]:::command
    RRC[ReleaseReservationCommand]:::command
    CRC[ConfirmReservationCommand]:::command
    INV(Inventory Service):::agg
    SR([StocksReserved]):::event
    SF([StocksNotReserved]):::event
    VC([VariantCreated]):::event
    PD([ProductDeleted]):::event

    MR --> INV
    RRC --> INV
    CRC --> INV
    INV --> SR
    INV --> SF
    VC --> INV
    PD --> INV
```

---

## Order Process Manager Integration

> Inventory is the **receiving side** of the Order service's Process Manager (`OrderSaga`). The saga drives this service with commands; Inventory replies with events. See the [Order Service README](../../../Order/src/EShop.Order.API/README.md) for the saga's own state machine and roadmap.

```mermaid
sequenceDiagram
    autonumber
    participant PM  as Order Process Manager
    participant INV as Inventory Service
    participant HOLD as Reservation (hold)

    PM->>INV: MakeReservation
    alt gate + CAS pass
        INV->>HOLD: create Pending hold (+15 min TTL)
        INV-->>PM: StocksReserved (ReservationId)
    else sold out / CAS rows = 0
        INV-->>PM: StocksNotReserved (reason)
    end

    rect rgb(232,245,233)
        Note over PM,HOLD: Payment-aware step — driven by Finance's reply (OrderPaymentScheduled / Failed)
        PM->>INV: ConfirmReservationCommand (payment scheduled)
        INV->>HOLD: Pending → Confirmed (no stock change)
        PM->>INV: ReleaseReservationCommand (payment schedule failed)
        INV->>HOLD: Pending → Released (atomic available += qty)
    end
```

| Saga sends | Inventory does | Reply | Status today |
|------------|----------------|-------|--------------|
| `MakeReservation` | Redis gate + CAS deduct + create `Pending` hold | `StocksReserved` / `StocksNotReserved` | ✅ Live |
| `ConfirmReservationCommand` | `Pending → Confirmed` (no stock change) | — | ✅ Live — issued when Finance replies `OrderPaymentScheduled` |
| `ReleaseReservationCommand` | `Pending → Released`, **atomic** stock add-back (`AddBackStockAsync`) + Redis compensation | — | ✅ Live — issued when Finance replies `OrderPaymentScheduleFailed` |

> Both commands are **idempotent** (guarded by `Status == Pending`) and **fire-and-forget** — the saga has already completed, so Inventory sends no reply. The release path adds stock back with an atomic SQL `UPDATE` (no lost updates under concurrent releases), inside a transaction with the `Pending → Released` status change, then compensates Redis after commit.

---

## Key Tables

| Table | One row per |
|-------|------------|
| `Inventories` | SKU x tenant |
| `Reservations` | Order x tenant — `UNIQUE(tenant_id, order_id)` |
| `ReservationItems` | Variant x reservation — `UNIQUE(reservation_id, variant_id)` |
| `OutboxMessages` | Pending event to publish |
| `inbox_messages` | Processed message (dedup) |
