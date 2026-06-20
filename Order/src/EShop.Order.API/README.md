# 🛒 Order Bounded Context

[![DDD](https://img.shields.io/badge/Pattern-Domain--Driven%20Design-blue)](/)
[![Saga](https://img.shields.io/badge/Pattern-Process%20Manager%20Saga-purple)](/)
[![Event Storming](https://img.shields.io/badge/Discovery-Event%20Storming-orange)](/)
[![CQRS](https://img.shields.io/badge/Pattern-CQRS-green)](/)

> **Order Aggregate** is the core aggregate root in the Order bounded context. It is created at the end of a **Place-Order Saga** — an orchestration process manager that coordinates stock reservation in the Inventory service before persisting the order.

---

## 📋 Table of Contents

- [Strategic Design](#-strategic-design)
- [Event Storming](#-event-storming)
- [Saga State Machine](#-saga-state-machine)
- [End-to-End Flow](#-end-to-end-flow)
- [Architecture — Process Manager Pattern](#-architecture--process-manager-pattern)
- [Message Contracts](#-message-contracts)
- [Tactical Design](#-tactical-design)
- [Key Design Decisions](#-key-design-decisions)

---

## 🗺 Strategic Design

### Bounded Context Map

```mermaid
graph TB
    subgraph Order["🛒 Order Bounded Context"]
        direction TB
        OA["Order Aggregate<br/>(Order + OrderItems)"]
        PM["PlaceOrderSagaOrchestrator<br/>(Process Manager)"]
    end

    subgraph Inventory["📦 Inventory Bounded Context"]
        SR["StockReservation Entity"]
        IG["IRedisStockGateway<br/>(Redis fast-gate)"]
    end

    subgraph Catalog["🏷 Catalog Bounded Context"]
        VA["Variant (SKU)"]
    end

    subgraph Downstream["📣 Downstream Consumers (future)"]
        NS["Notification Service"]
        FS["Finance Service"]
    end

    Order -->|"ReserveStockCommand<br/>(saga command)"| Inventory
    Inventory -->|"StockReserved / StockReservationFailed<br/>(integration events)"| Order
    Catalog -.->|"VariantId reference<br/>(read-only)"| Order
    Order -->|"OrderAccepted / OrderRejected<br/>(integration events)"| Downstream

    style Order fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style Inventory fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px
    style Catalog fill:#fff3e0,stroke:#e65100,stroke-width:1px,stroke-dasharray:4
    style Downstream fill:#f3e5f5,stroke:#6a1b9a,stroke-width:1px,stroke-dasharray:4
```

### Context Classification

| Aspect | Description |
|:-------|:------------|
| **Bounded Context** | Order |
| **Aggregate Root** | `Order` |
| **Domain Type** | Core Domain |
| **Persistence** | EF Core + PostgreSQL (write side) |
| **Saga Persistence** | `SagaStates` table — `PlaceOrderSagaState` (EF Core) |
| **Multi-tenancy** | `IExcludedFromScoping` — tenant propagated through saga context |
| **Saga Pattern** | Orchestration — Process Manager (no MassTransit native state machine) |

### Ubiquitous Language

| Term | Definition |
|:-----|:-----------|
| **Order** | A confirmed purchase intent, containing one or more OrderItems |
| **OrderItem** | A line item referencing a Catalog Variant (SKU), with quantity and price snapshot |
| **Saga** | Long-running process manager coordinating Order + Inventory across a distributed transaction |
| **Stock Reservation** | A temporary hold on inventory units pending order confirmation (TTL: 15 min) |
| **ReservationId** | A handle returned by Inventory identifying the reservation row — stored in saga state |
| **IdempotencyKey** | Equals `OrderId`; used by Inventory to detect duplicate `ReserveStockCommand` retries |
| **OrderSubmitted** | Trigger event that starts the saga — published by `PlaceOrderCommandHandler` |
| **OrderAccepted** | Terminal success event — stock reserved + order row persisted |
| **OrderRejected** | Terminal failure event — insufficient stock or saga timed out |

---

## 🔶 Event Storming

> **Phase 1 — Collaborative Discovery.** Event Storming maps the entire Place-Order flow across service boundaries. Notation follows [Alberto Brandolini's](https://www.eventstorming.com/) sticky-note convention.

### Legend

```mermaid
graph LR
    A["👤 Actor"]:::actor -- issues a --> C["Command"]:::command
    C -- handled by --> AGG["Aggregate / Handler"]:::aggregate
    AGG -- produces a --> E["Domain / Integration Event"]:::event
    E -- triggers --> P["Policy / Process Manager"]:::policy
    P -- issues a --> C2["Command"]:::command
    E -- updates a --> R["Read Model"]:::readmodel
    H["Hotspot ❓"]:::hotspot

    classDef actor    fill:#fff176,stroke:#f9a825,color:#000
    classDef command  fill:#42a5f5,stroke:#1565c0,color:#fff
    classDef aggregate fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef event    fill:#ff9800,stroke:#e65100,color:#fff
    classDef policy   fill:#ce93d8,stroke:#7b1fa2,color:#000
    classDef readmodel fill:#a5d6a7,stroke:#2e7d32,color:#000
    classDef hotspot  fill:#f06292,stroke:#c2185b,color:#fff
```

### Actors

```mermaid
graph TB
    subgraph Actors["👤 Actors in Order Domain"]
        direction LR
        A1["👤 Buyer<br/>━━━━━━━━━━<br/>End customer<br/>Places orders"]:::actor
        A2["⚙️ Saga Orchestrator<br/>━━━━━━━━━━<br/>PlaceOrderSagaOrchestrator<br/>Coordinates cross-service flow"]:::system
        A3["⚙️ Inventory Service<br/>━━━━━━━━━━<br/>Reserves / releases stock<br/>Publishes reservation events"]:::system
        A4["⏱ Hangfire Job<br/>━━━━━━━━━━<br/>ExpireReservationsJob<br/>Compensates expired TTLs"]:::system
    end

    classDef actor  fill:#fff176,stroke:#f9a825,color:#000
    classDef system fill:#e0e0e0,stroke:#616161,color:#000
```

### Place-Order — Full Event Storm

```mermaid
graph LR
    BUYER["👤 Buyer"]:::actor

    C1["PlaceOrderCommand"]:::command
    C2["ReserveStockCommand"]:::command
    C3["PersistOrderCommand"]:::command
    C4["ReleaseReservationCommand"]:::command

    H1["PlaceOrderCommandHandler"]:::aggregate
    AGG_INV["ReserveStockConsumer<br/>(Inventory)"]:::aggregate
    AGG_ORD["PersistOrderConsumer<br/>(Order)"]:::aggregate
    AGG_REL["ReleaseReservationConsumer<br/>(Inventory)"]:::aggregate

    E1["OrderSubmitted"]:::event
    E2["StockReserved"]:::event
    E3["StockReservationFailed"]:::event
    E4["OrderPersisted"]:::event
    E5["OrderAccepted"]:::event
    E6["OrderRejected"]:::event

    PM["PlaceOrderSagaOrchestrator<br/>━━━━━━━━━━━━━━━━━━━━━<br/>Process Manager<br/>(pure C# — no MassTransit)"]:::policy

    RM["SagaStates Table<br/>(PostgreSQL)"]:::readmodel
    RM2["Orders Table<br/>(PostgreSQL)"]:::readmodel

    BUYER --> C1 --> H1 --> E1
    E1 --> PM
    PM -->|"Initial → AwaitingStockReservation"| C2
    PM -.->|"persist state"| RM
    C2 --> AGG_INV
    AGG_INV --> E2
    AGG_INV --> E3

    E2 --> PM
    PM -->|"AwaitingStockReservation → AwaitingOrderPersistence"| C3
    C3 --> AGG_ORD --> E4
    E4 --> PM
    PM -->|"AwaitingOrderPersistence → Completed"| E5
    AGG_ORD -.->|"persist order"| RM2

    E3 --> PM
    PM -->|"AwaitingStockReservation → Failed"| E6
    PM -->|"compensation"| C4
    C4 --> AGG_REL

    classDef actor    fill:#fff176,stroke:#f9a825,color:#000
    classDef command  fill:#42a5f5,stroke:#1565c0,color:#fff
    classDef aggregate fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef event    fill:#ff9800,stroke:#e65100,color:#fff
    classDef policy   fill:#ce93d8,stroke:#7b1fa2,color:#000
    classDef readmodel fill:#a5d6a7,stroke:#2e7d32,color:#000
```

### Expiry Compensation — Event Storm

```mermaid
graph LR
    JOB["⏱ ExpireReservationsJob<br/>(Hangfire — every 1 min)"]:::system

    C1["reservation.Expire()"]:::aggregate
    C2["Redis.Release(qty)"]:::aggregate

    E1["ReservationExpired"]:::event

    INV_DB["StockReservations Table<br/>(PostgreSQL)"]:::readmodel
    REDIS["Redis Counters<br/>stock:available / stock:reserved"]:::readmodel

    JOB -->|"Status=Active AND ExpiresAt ≤ now"| C1
    C1 --> E1
    C1 -.->|"Status → Expired"| INV_DB
    C2 -.->|"INCRBY available / DECRBY reserved"| REDIS
    E1 --> C2

    classDef system   fill:#e0e0e0,stroke:#616161,color:#000
    classDef aggregate fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef event    fill:#ff9800,stroke:#e65100,color:#fff
    classDef readmodel fill:#a5d6a7,stroke:#2e7d32,color:#000
```

---

## 🔄 Saga State Machine

### State Diagram

```mermaid
stateDiagram-v2
    [*] --> Initial : PlaceOrderSagaState created

    Initial --> AwaitingStockReservation : OrderSubmitted\n→ sends ReserveStockCommand

    AwaitingStockReservation --> AwaitingOrderPersistence : StockReserved\n→ sends PersistOrderCommand
    AwaitingStockReservation --> Failed : StockReservationFailed\n→ publishes OrderRejected

    AwaitingOrderPersistence --> Completed : OrderPersisted\n→ publishes OrderAccepted

    Completed --> [*]
    Failed --> [*]

    state AwaitingStockReservation {
        [*] --> reserving
        reserving : ReserveStockCommand sent to Inventory
        reserving : Redis Lua fast-gate running
        reserving : Postgres tx pending
    }

    state AwaitingOrderPersistence {
        [*] --> persisting
        persisting : PersistOrderCommand sent to Order write side
        persisting : Order.CreateOrder() running
        persisting : EF Core SaveChanges pending
    }

    state Completed {
        [*] --> done
        done : OrderAccepted published
        done : Saga row retained for audit
    }

    state Failed {
        [*] --> rejected
        rejected : OrderRejected published
        rejected : No stock was deducted
        rejected : Saga row retained for audit
    }
```

### State Reference

| State | Entry trigger | Exit triggers | Side effects on entry |
|:------|:-------------|:-------------|:----------------------|
| `Initial` | Row created | `OrderSubmitted` | — |
| `AwaitingStockReservation` | `OrderSubmitted` | `StockReserved`, `StockReservationFailed` | `ReserveStockCommand` sent |
| `AwaitingOrderPersistence` | `StockReserved` | `OrderPersisted` | `PersistOrderCommand` sent |
| `Completed` | `OrderPersisted` | — (terminal) | `OrderAccepted` published |
| `Failed` | `StockReservationFailed` | — (terminal) | `OrderRejected` published |

---

## ⚡ End-to-End Flow

### Happy Path — Sequence

```mermaid
sequenceDiagram
    participant Buyer
    participant OrderAPI as Order API
    participant Handler as PlaceOrderCommandHandler
    participant PM as PlaceOrderSagaOrchestrator
    participant InvSvc as Inventory Service
    participant OrdWrite as Order Write Side

    Buyer->>OrderAPI: POST /api/v1/orders
    OrderAPI->>Handler: PlaceOrderCommand
    Handler->>Handler: generate OrderId
    Handler-->>OrderAPI: publishes OrderSubmitted
    OrderAPI-->>Buyer: 202 Accepted { orderId }

    Note over PM: Saga row created (Initial)
    PM->>PM: OnOrderSubmitted → state = AwaitingStockReservation
    PM->>InvSvc: ReserveStockCommand

    Note over InvSvc: Redis Lua fast-gate (atomic all-or-nothing)
    Note over InvSvc: Postgres tx: StockAvailable -= qty, insert StockReservation
    InvSvc-->>PM: StockReserved { reservationId }

    PM->>PM: OnStockReserved → state = AwaitingOrderPersistence
    PM->>OrdWrite: PersistOrderCommand

    Note over OrdWrite: idempotency check
    Note over OrdWrite: Order.CreateOrder → EF SaveChanges
    OrdWrite-->>PM: OrderPersisted

    PM->>PM: OnOrderPersisted → state = Completed
    PM-->>Buyer: OrderAccepted (published — via downstream consumer)
```

### Failure Path — Insufficient Stock

```mermaid
sequenceDiagram
    participant Buyer
    participant OrderAPI as Order API
    participant PM as PlaceOrderSagaOrchestrator
    participant InvSvc as Inventory Service

    Buyer->>OrderAPI: POST /api/v1/orders
    OrderAPI-->>Buyer: 202 Accepted { orderId }

    PM->>InvSvc: ReserveStockCommand

    Note over InvSvc: Redis Lua: available < requested quantity
    InvSvc-->>PM: StockReservationFailed { reason }

    PM->>PM: OnStockReservationFailed → state = Failed
    PM-->>Buyer: OrderRejected (published — downstream Notification)
```

### Compensation — Reservation Expiry

```mermaid
sequenceDiagram
    participant Hangfire as ExpireReservationsJob (Hangfire)
    participant InvDB as Inventory PostgreSQL
    participant Redis as Redis

    loop Every 1 minute
        Hangfire->>InvDB: SELECT WHERE Status=Active AND ExpiresAt ≤ now
        InvDB-->>Hangfire: expired reservations[]

        loop per reservation
            Hangfire->>InvDB: reservation.Expire() → Status = Expired
            Hangfire->>Redis: INCRBY available, DECRBY reserved (Lua)
        end

        Hangfire->>InvDB: SaveChangesAsync
    end
```

---

## 🏗 Architecture — Process Manager Pattern

### Layer Diagram

```mermaid
graph TB
    subgraph Application["EShop.Order.Application — zero MassTransit dependency"]
        direction TB
        STATE["PlaceOrderSagaState<br/>(plain POCO)"]
        ORCH["PlaceOrderSagaOrchestrator<br/>(pure C# — On* methods)"]
        RESULT["SagaTransitionResult<br/>(Commands + Events)"]
        IREPOSITORY["ISagaStateRepository<br/>(persistence abstraction)"]
        STATES["SagaStates<br/>(string constants)"]
    end

    subgraph Infrastructure["EShop.Order.Infrastructure — MassTransit consumers"]
        direction TB
        C1["OrderSubmittedConsumer<br/>IConsumer&lt;OrderSubmitted&gt;"]
        C2["StockReservedConsumer<br/>IConsumer&lt;StockReserved&gt;"]
        C3["StockReservationFailedConsumer<br/>IConsumer&lt;StockReservationFailed&gt;"]
        C4["OrderPersistedConsumer<br/>IConsumer&lt;OrderPersisted&gt;"]
        C5["PersistOrderConsumer<br/>IConsumer&lt;PersistOrderCommand&gt;"]
        REPO["SagaStateRepository<br/>(EF Core impl of ISagaStateRepository)"]
        DBCTX["OrderDbContext<br/>SagaStates + Orders + OrderItems"]
    end

    Infrastructure -->|"depends on"| Application
    ORCH --> RESULT
    ORCH --> STATE
    REPO -->|"implements"| IREPOSITORY
    C1 & C2 & C3 & C4 -->|"loads/saves via"| IREPOSITORY
    C1 & C2 & C3 & C4 -->|"delegates to"| ORCH
    REPO --> DBCTX

    style Application fill:#e3f2fd,stroke:#1565c0,stroke-width:2px
    style Infrastructure fill:#fff3e0,stroke:#e65100,stroke-width:2px
```

### Write Model — Command → Event Flow

```mermaid
flowchart LR
    subgraph API["🌐 API Layer"]
        EP["POST /api/v1/orders<br/>(Minimal API)"]
    end

    subgraph Application["⚙️ Application Layer"]
        CMD["PlaceOrderCommand"]
        HANDLER["PlaceOrderCommandHandler"]
        ORCH2["PlaceOrderSagaOrchestrator<br/>(pure C#)"]
        STATE2["PlaceOrderSagaState"]
    end

    subgraph Infrastructure["🗄 Infrastructure"]
        CONSUMERS["Saga Consumers<br/>(thin MassTransit adapters)"]
        REPO2["SagaStateRepository<br/>(EF Core)"]
        DB["PostgreSQL<br/>SagaStates + Orders"]
        MQ["RabbitMQ<br/>(MassTransit)"]
    end

    EP -->|"map request"| CMD
    CMD -->|"handled by"| HANDLER
    HANDLER -->|"publish OrderSubmitted"| MQ
    MQ -->|"consumed by"| CONSUMERS
    CONSUMERS -->|"load state"| REPO2
    CONSUMERS -->|"call On*(state, msg)"| ORCH2
    ORCH2 -->|"mutate"| STATE2
    ORCH2 -->|"return SagaTransitionResult"| CONSUMERS
    CONSUMERS -->|"save"| REPO2
    REPO2 --> DB
    CONSUMERS -->|"dispatch commands/events"| MQ

    style API fill:#e3f2fd,stroke:#1565c0
    style Application fill:#fff3e0,stroke:#e65100
    style Infrastructure fill:#e8f5e9,stroke:#2e7d32
```

---

## 📨 Message Contracts

All contracts live in `EShop.Shared.Contracts` — shared between Order and Inventory services.

### Contract Flow Map

```mermaid
graph LR
    subgraph OrderSvc["Order Service"]
        H["PlaceOrderCommandHandler"]
        OSC["OrderSubmittedConsumer"]
        SRC["StockReservedConsumer"]
        SRFC["StockReservationFailedConsumer"]
        OPC["OrderPersistedConsumer"]
        POC["PersistOrderConsumer"]
    end

    subgraph InventorySvc["Inventory Service"]
        RSC["ReserveStockConsumer"]
        RRC["ReleaseReservationConsumer"]
    end

    subgraph Bus["RabbitMQ"]
        OS["OrderSubmitted"]:::event
        RSCmd["ReserveStockCommand"]:::command
        StockR["StockReserved"]:::event
        StockF["StockReservationFailed"]:::event
        POCmd["PersistOrderCommand"]:::command
        OP["OrderPersisted"]:::event
        OA["OrderAccepted"]:::event
        OR["OrderRejected"]:::event
        RRCmd["ReleaseReservationCommand"]:::command
    end

    H -->|publishes| OS
    OS -->|consumed by| OSC
    OSC -->|publishes| RSCmd
    RSCmd -->|consumed by| RSC
    RSC -->|publishes| StockR
    RSC -->|publishes| StockF
    StockR -->|consumed by| SRC
    StockF -->|consumed by| SRFC
    SRC -->|publishes| POCmd
    SRFC -->|publishes| OR
    SRFC -->|publishes| RRCmd
    POCmd -->|consumed by| POC
    POC -->|publishes| OP
    OP -->|consumed by| OPC
    OPC -->|publishes| OA
    RRCmd -->|consumed by| RRC

    classDef event   fill:#ff9800,stroke:#e65100,color:#fff
    classDef command fill:#42a5f5,stroke:#1565c0,color:#fff
```

### Contract Reference

| Message | Type | Publisher | Consumer |
|:--------|:-----|:----------|:---------|
| `OrderSubmitted` | Trigger event | `PlaceOrderCommandHandler` | `OrderSubmittedConsumer` |
| `ReserveStockCommand` | Saga command | `OrderSubmittedConsumer` | `ReserveStockConsumer` |
| `StockReserved` | Integration event | `ReserveStockConsumer` | `StockReservedConsumer` |
| `StockReservationFailed` | Integration event | `ReserveStockConsumer` | `StockReservationFailedConsumer` |
| `PersistOrderCommand` | Saga command | `StockReservedConsumer` | `PersistOrderConsumer` |
| `OrderPersisted` | Internal event | `PersistOrderConsumer` | `OrderPersistedConsumer` |
| `OrderAccepted` | Integration event | `OrderPersistedConsumer` | downstream |
| `OrderRejected` | Integration event | `StockReservationFailedConsumer` | downstream |
| `ReleaseReservationCommand` | Compensation command | `StockReservationFailedConsumer` | `ReleaseReservationConsumer` |
| `ReservationExpired` | Integration event | `ExpireReservationsJob` | (future) |

---

## 🧱 Tactical Design

### Aggregate Structure

```mermaid
classDiagram
    class Order {
        <<Aggregate Root>>
        Guid Id
        string BuyerId
        DateTimeOffset OrderDate
        string Status
        string? Description
        DateTimeOffset CreatedAtUtc
        DateTimeOffset? LastModifiedAtUtc
        +CreateOrder(PlaceOrderCommand) Order$
        +AddOrderItems(items)
    }

    class OrderItem {
        <<Entity>>
        Guid Id
        Guid OrderId
        Guid VariantId
        int Quantity
        decimal UnitPrice
        decimal? Discount
        decimal TotalPrice
    }

    class OrderStatus {
        <<Enumeration>>
        Pending
        Created
        Rejected
        Processing
        Shipped
        Delivered
        Cancelled
    }

    class PlaceOrderSagaState {
        <<Process Manager State>>
        Guid CorrelationId
        string CurrentState
        string BuyerId
        string TenantId
        string OrderItemsJson
        Guid? ReservationId
        string? FailureReason
        DateTimeOffset SubmittedAt
        byte[] RowVersion
    }

    Order "1" *-- "1..*" OrderItem : contains
    Order --> OrderStatus : status
    PlaceOrderSagaState ..> Order : creates via PersistOrderCommand
```

### Inventory — Supporting Entities

```mermaid
classDiagram
    class StockReservation {
        <<Entity>>
        Guid Id
        Guid OrderId
        Guid VariantId
        int Quantity
        Guid IdempotencyKey
        ReservationStatus Status
        DateTimeOffset ExpiresAt
        DateTimeOffset CreatedAtUtc
        DateTimeOffset? ReleasedAtUtc
        +Create(orderId, variantId, qty, idempotencyKey, expiresAt)$
        +Release()
        +Expire()
        +Confirm()
    }

    class ReservationStatus {
        <<Enumeration>>
        Active
        Released
        Expired
        Confirmed
    }

    StockReservation --> ReservationStatus : status
```

---

## 🛡 Key Design Decisions

```mermaid
graph TB
    subgraph Decisions["Key Design Decisions"]
        D1["🔵 Process Manager over<br/>MassTransit native saga<br/>━━━━━━━━━━━━━━<br/>Orchestrator is pure C#<br/>No framework lock-in<br/>Fully unit-testable"]
        D2["🟠 202 Accepted immediately<br/>━━━━━━━━━━━━━━<br/>Client gets orderId before<br/>stock reservation completes<br/>Async by design"]
        D3["🟢 Redis fast-gate + Postgres<br/>━━━━━━━━━━━━━━<br/>Redis Lua: atomic all-or-nothing<br/>Postgres: authoritative truth<br/>Avoids DB load under contention"]
        D4["🟣 Idempotency everywhere<br/>━━━━━━━━━━━━━━<br/>PersistOrderConsumer: order exists check<br/>ReserveStockConsumer: IdempotencyKey<br/>SagaOrchestrator: NoOp() guard"]
        D5["🔴 Optimistic concurrency<br/>━━━━━━━━━━━━━━<br/>RowVersion on SagaStates<br/>Prevents two consumers<br/>advancing same saga"]
        D6["⏱ TTL = 15 min / job = 1 min<br/>━━━━━━━━━━━━━━<br/>Hangfire ExpireReservationsJob<br/>Compensates Redis on expiry<br/>Status: Active → Expired"]
    end

    style D1 fill:#e3f2fd,stroke:#1565c0
    style D2 fill:#fff3e0,stroke:#e65100
    style D3 fill:#e8f5e9,stroke:#2e7d32
    style D4 fill:#f3e5f5,stroke:#6a1b9a
    style D5 fill:#ffebee,stroke:#c62828
    style D6 fill:#f9fbe7,stroke:#558b2f
```

| Decision | Rationale |
|:---------|:----------|
| **Process Manager over MassTransit native saga** | `PlaceOrderSagaOrchestrator` has zero dependency on MassTransit. It is a plain, unit-testable class. MassTransit is confined to the Infrastructure boundary. |
| **202 Accepted immediately** | Client receives `orderId` before reservation completes. Async by design — subscribe to `OrderAccepted` / `OrderRejected` downstream. |
| **Redis fast-gate before Postgres** | Lua script performs atomic all-or-nothing check. Only on Redis pass does a Postgres transaction run — avoids unnecessary DB load under contention. |
| **Idempotency in every consumer** | `PersistOrderConsumer` checks for existing order row. `ReserveStockConsumer` checks `IdempotencyKey`. `OnXxx` returns `NoOp()` when saga is not in expected state. Safe to replay on MassTransit retry. |
| **Optimistic concurrency on saga state** | `PlaceOrderSagaState.RowVersion` prevents two concurrent consumers advancing the same saga simultaneously. |
| **Reservation TTL = 15 minutes** | `ExpireReservationsJob` (Hangfire, every 1 min) scans `Status=Active AND ExpiresAt ≤ now`, calls `Expire()`, compensates Redis counters. |
