# Finance

> Owns the **payment lifecycle of an order**. On a `MakePayment` command from the Order saga it opens a finance **account**, turns the order total into a **payment schedule** (pay up front or spread over monthly / quarterly / annual payments), and **replies to the saga** so the order can advance.

> Order side of this contract: [Order Service README](../../../Order/src/EShop.Order.API/README.md)
>
> **Scope today:** create account → calculate & schedule payments → reply to the Order saga.
> **Deferred (next ticket):** *booking* — pushing each payment to the tenant's external accounting provider and recording collected payments.

---

## What This Service Does

```mermaid
graph LR
    classDef pm   fill:#ce93d8,stroke:#7b1fa2,color:#000
    classDef agg  fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef ext  fill:#e1bee7,stroke:#6a1b9a,color:#000

    ORD([Order Saga]):::pm
    FIN(Finance Service):::agg
    EXT([Tenant's accounting /<br/>payment API]):::ext

    ORD ==>|"MakePayment (command)"| FIN
    FIN ==>|"OrderPaymentScheduled /<br/>OrderPaymentScheduleFailed"| ORD
    FIN -.->|"book payment — deferred"| EXT
```

---

## Strategic Design

### Context Classification

| Aspect | Value |
|--------|-------|
| **Bounded Context** | Finance |
| **Domain Type** | Core Domain |
| **Aggregate Roots** | `Account` (with `Payment` child) |
| **Multi-tenancy** | `IScoped` — `Account` and `Payment` carry `TenantId`/`Scope` (EF Core global query filters) |
| **Persistence** | EF Core (PostgreSQL) — state-based (not event-sourced) |
| **Read Model** | None |
| **Architecture Style** | Clean Architecture + Strategy pattern for schedule calculation |

### Bounded Context Map

```mermaid
graph TB
    classDef ctx fill:#eaf2f8,stroke:#1a5276,color:#000

    subgraph OrderCtx["Order Context"]
        ORD[OrderSaga]
    end
    subgraph FinCtx["Finance Context (Core)"]
        FIN[Account]
    end
    subgraph ExtCtx["External Provider (deferred)"]
        EXT[Accounting / payment API]
    end

    ORD -->|"MakePayment<br/>(Customer–Supplier)"| FIN
    FIN -->|"OrderPaymentScheduled / OrderPaymentScheduleFailed"| ORD
    FIN -.->|"book payment (next ticket)<br/>(Anti-Corruption Layer)"| EXT

    class OrderCtx,FinCtx,ExtCtx ctx
```

### Ubiquitous Language

| Term | Definition |
|------|------------|
| **Account** | The finance record owning an order's payment lifecycle: total, currency, frequency, status, and its scheduled payments. One per `(TenantId, OrderId)`. |
| **Payment** | One scheduled instalment within an account — amount + due date + status. |
| **Payment schedule** | The set of payments produced from the total and `PaymentFrequency`. |
| **PaymentFrequency** | How the total is split: `OneOff`, `Monthly`, `Quarterly`, `Annually`. |
| **Schedule integrity** | The invariant that the payments sum **exactly** to the account total. |
| **Outstanding amount** | What remains unpaid; reduced as payments are recorded (booking ticket). |
| **Booking** | Pushing a payment to the tenant's external accounting provider — deferred to a later ticket. |

---

## Event Storming

### Participants & Roles

| Role | Contribution | Artifact Ownership |
|------|--------------|--------------------|
| **Product Owner** | Defines the payment frequencies and what "scheduled" means for the order | Ubiquitous Language, Policies |
| **Business Analyst** | Clarifies rounding rules, invalid-total handling, redelivery | Hotspots, edge cases |
| **Solution Architect** | Validates the saga reply contract and the strategy boundary | Aggregate boundaries, Context Map |
| **Developer** | Implements the strategy calculator, the account aggregate, and the consumer | Commands, Events, Specifications |

### Legend

```mermaid
graph LR
    A["👤 Actor"]:::actor -- issues --> C["Command"]:::command
    C -- handled by --> AGG["Aggregate"]:::aggregate
    AGG -- emits --> E["Domain / Integration Event"]:::event
    E -- triggers --> P["Policy"]:::policy
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
    subgraph Actors["👤 Actors"]
        direction LR
        SAGA["Order Saga<br/>asks Finance to schedule payment"]
        PROV["External provider<br/>(next ticket) — collects payment"]
    end
```

| Actor | Interacts With | Example Scenario |
|-------|----------------|------------------|
| **Order Saga** | `Account` aggregate | *As the saga, I issue `MakePayment` and react to the scheduled/failed reply.* |
| **External provider** | `Account` (deferred) | *As the provider, I confirm collected payments (booking ticket).* |

### Account — Event Flow

```mermaid
graph LR
    SAGA([Order Saga]):::policy --> MP[MakePayment]:::command
    MP --> CAC[CreateAccountCommand]:::command --> ACC(Account):::aggregate
    ACC --> AC([AccountCreated]):::event
    ACC --> PSch([PaymentScheduled]):::event
    ACC --> OPS([OrderPaymentScheduled]):::event --> SAGA
    ACC --> OPF([OrderPaymentScheduleFailed]):::event --> SAGA
    PB([PaymentBooked ❓]):::hotspot
    PP([PaymentPaid ❓]):::hotspot
    ACdone([AccountCompleted ❓]):::hotspot

    classDef command  fill:#42a5f5,stroke:#1565c0,color:#fff
    classDef aggregate fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef event    fill:#ff9800,stroke:#e65100,color:#fff
    classDef policy   fill:#ce93d8,stroke:#7b1fa2,color:#000
    classDef hotspot  fill:#f06292,stroke:#c2185b,color:#fff
```

> `PaymentBooked` / `PaymentPaid` / `AccountCompleted` are domain events the aggregate can raise but **no orchestration drives them yet** — they belong to the booking ticket.

### Policies — When / Then Rules

| When this command/event | Then | Rail / Transport |
|-------------------------|------|------------------|
| `MakePayment` | `CreateAccountCommand` → create account + calculate schedule | MassTransit consumer → MediatR |
| Schedule built | publish `OrderPaymentScheduled` | `IEventBus` → RabbitMQ |
| Invalid total / frequency | publish `OrderPaymentScheduleFailed` | `IEventBus` → RabbitMQ |

---

## Core Concept — Payment Schedule (Strategy pattern)

The order total is split into **payments** by `PaymentFrequency` using a **Strategy pattern**: one
[`IPaymentScheduleStrategy`](../EShop.Finance.Domain/Services/PaymentSchedule/IPaymentScheduleStrategy.cs)
per frequency, selected by
[`PaymentScheduleStrategyFactory`](../EShop.Finance.Domain/Services/PaymentSchedule/PaymentScheduleStrategyFactory.cs)
and orchestrated by the
[`PaymentScheduleCalculator`](../EShop.Finance.Domain/Services/PaymentSchedule/PaymentScheduleCalculator.cs)
domain service. Adding a frequency = adding a strategy (Open/Closed); the shared even-split + rounding rule lives once in the base strategy. Pure and fully unit-tested.

| Frequency  | Payments | Interval        |
|------------|----------|-----------------|
| `OneOff`   | 1        | —               |
| `Monthly`  | 12       | +1 month        |
| `Quarterly`| 4        | +3 months       |
| `Annually` | 1        | (one-year term) |

```mermaid
flowchart LR
    T["Total = 120.00<br/>Quarterly"] --> C[PaymentScheduleCalculator]
    C --> F{{PaymentScheduleStrategyFactory}}
    F --> S[QuarterlyPaymentScheduleStrategy]
    S --> I1["#1 30.00 — Jan 15"]
    S --> I2["#2 30.00 — Apr 15"]
    S --> I3["#3 30.00 — Jul 15"]
    S --> I4["#4 30.00 — Oct 15"]
```

Rules (enforced by tests **and** re-checked by the aggregate):

- Amounts are split **evenly at the currency minor unit**; the rounding remainder lands on the **final** payment, so payments always sum to the total (e.g. `100.00` monthly → 11 × `8.33` + `8.37`).
- The first payment is due on the start date; each subsequent one advances by the frequency interval.
- A zero/negative total or an unsupported frequency is rejected with a `DomainException`.
- `Account.CalculateScheduledPayments` calls `AssertScheduleIntegrity()` — the aggregate verifies the sum itself rather than trusting the calculator.

---

## Domain Model

### Aggregate Structure

```mermaid
classDiagram
    class Account {
        <<Aggregate Root>>
        Guid Id
        Guid OrderId
        string BuyerId
        decimal TotalAmount
        string Currency
        string PaymentFrequency
        AccountStatus Status
        decimal OutstandingAmount
        +Create()
        +CalculateScheduledPayments(startDate)
        +Fail(reason)
        +BookPayment(id, ref)
        +RecordPayment(id, amount, paidAt)
    }
    class Payment {
        <<Entity>>
        Guid Id
        Guid AccountId
        int Sequence
        decimal Amount
        DateOnly DueDate
        PaymentStatus Status
        string ExternalBookingReference
    }
    class ScheduledPayment {
        <<Value Object>>
        int Sequence
        decimal Amount
        DateOnly DueDate
    }
    class PaymentFrequency {
        <<Enumeration>>
        OneOff
        Monthly
        Quarterly
        Annually
    }

    Account "1" --> "1..*" Payment : Payments
    PaymentScheduleCalculator ..> ScheduledPayment : produces
    Account ..> ScheduledPayment : materialises into Payment
```

### Building Blocks

| Building Block | Type | Identity | Rationale |
|----------------|------|----------|-----------|
| `Account` | **Aggregate Root** | `Guid Id` (one per `TenantId, OrderId`) | Consistency boundary for an order's payments. |
| `Payment` | **Entity** | `Guid Id` (child of `Account`) | A scheduled instalment; meaningful only inside its account. |
| `ScheduledPayment` | **Value Object** | By attributes | The calculator's pure output `(Sequence, Amount, DueDate)` before it becomes a `Payment`. |
| `PaymentFrequency` / `AccountStatus` / `PaymentStatus` | **Enumeration** | Enum value | Frequency vocabulary and lifecycle states. |
| `IPaymentScheduleStrategy` (+ per-frequency strategies, factory) | **Domain Service** | Stateless | Encapsulates per-frequency split rules; DI-free, Open/Closed. |
| `AccountCreated`, `PaymentScheduled`, `PaymentBooked`, `PaymentPaid`, `AccountCompleted`, `AccountFailed` | **Domain Event** | By attributes | Facts the aggregate raises (the last three are booking-ticket). |

---

## State Machines

Both lifecycles are status-driven (guarded methods on the aggregate/entity), not Stateless machines.

### Account Status

```mermaid
stateDiagram-v2
    [*] --> AwaitingSchedule : Create (from MakePayment)
    AwaitingSchedule --> Scheduled : CalculateScheduledPayments (sum == total)
    Scheduled --> Completed : all payments paid (booking ticket)
    AwaitingSchedule --> Failed : Fail(reason) (booking ticket)
    Scheduled --> Failed : Fail(reason) (booking ticket)
    Scheduled --> [*]
    Completed --> [*]
    Failed --> [*]
```

> Today the persisted path is `AwaitingSchedule → Scheduled`. An invalid total/frequency throws **before** the account is saved, so the handler replies `OrderPaymentScheduleFailed` without persisting an account. `Completed` / `Failed` are domain-ready for the booking ticket.

### Payment Status

```mermaid
stateDiagram-v2
    [*] --> Pending : CalculateScheduledPayments
    Pending --> Booked : MarkBooked(ref) (booking ticket)
    Booked --> Paid : MarkPaid(amount, at) (booking ticket)
    Booked --> Failed : MarkFailed (booking ticket)
    Paid --> [*]
```

---

## Specifications & Invariants

Finance enforces its invariants inside the aggregate and the pure calculator — there are no separate `Specification` classes.

| Invariant | Mechanism | Guard |
|-----------|-----------|-------|
| Payments sum exactly to the total | `Account.AssertScheduleIntegrity()` | Aggregate |
| Schedule generated once | `Status == AwaitingSchedule` guard in `CalculateScheduledPayments` | Aggregate |
| Total must be positive | `PaymentScheduleCalculator` throws `DomainException` | Domain service |
| Frequency must be supported | `PaymentScheduleStrategyFactory.Resolve` throws `DomainException` | Domain service |
| One account per order | `FindByOrderIdAsync` idempotency check + `UNIQUE(tenant_id, order_id)` | Application + Database |
| Valid payment transitions | `Payment.MarkBooked/MarkPaid` guards (booking ticket) | Entity |

### Invariant Enforcement Flow

```mermaid
sequenceDiagram
    participant H as CreateAccountCommandHandler
    participant ACC as Account
    participant CALC as PaymentScheduleCalculator

    H->>ACC: Account.Create(...) + CalculateScheduledPayments(today)
    ACC->>CALC: Calculate(total, frequency, startDate)
    alt total <= 0 or unsupported frequency
        CALC-->>ACC: throw DomainException
        ACC-->>H: bubbles up → publish OrderPaymentScheduleFailed
    else valid
        CALC-->>ACC: ScheduledPayment[]
        ACC->>ACC: materialise Payments + AssertScheduleIntegrity()
        ACC-->>H: Scheduled → persist + publish OrderPaymentScheduled
    end
```

---

## Architecture

### Layer Overview

```mermaid
flowchart TB
    subgraph API["🌐 API Layer"]
        EP["AccountEndpoints (read) · Startup · Swagger"]
    end
    subgraph Application["⚙️ Application Layer"]
        CMD["CreateAccountCommandHandler"]
    end
    subgraph Domain["🧠 Domain Layer"]
        AGG["Account · Payment"]
        SVC["PaymentScheduleCalculator + strategies"]
    end
    subgraph Infrastructure["🗄 Infrastructure Layer"]
        REPO["AccountRepository"]
        CON["MakePaymentConsumer"]
        BUS["IEventBus (reply)"]
        DB["EF Core / FinanceDbContext (PostgreSQL)"]
    end

    EP --> REPO
    CON --> CMD
    CMD --> AGG
    AGG --> SVC
    CMD --> REPO
    CMD --> BUS
    REPO --> DB
```

### Happy Path — Schedule Payment

```mermaid
sequenceDiagram
    autonumber
    participant ORD as Order Saga
    participant C as MakePaymentConsumer
    participant H as CreateAccountCommandHandler
    participant ACC as Account
    participant DB as PostgreSQL

    ORD->>C: MakePayment(orderId, total, currency, frequency)
    C->>H: CreateAccountCommand
    H->>H: FindByOrderIdAsync (idempotency)
    H->>ACC: Account.Create + CalculateScheduledPayments
    ACC-->>H: Scheduled (N payments)
    H->>DB: persist account + payments
    H-->>ORD: OrderPaymentScheduled(orderId, accountId, paymentCount)
```

### Compensation — Invalid Schedule

```mermaid
sequenceDiagram
    autonumber
    participant ORD as Order Saga
    participant H as CreateAccountCommandHandler
    participant ACC as Account

    ORD->>H: MakePayment (total <= 0 or bad frequency)
    H->>ACC: CalculateScheduledPayments → DomainException
    H-->>ORD: OrderPaymentScheduleFailed(orderId, reason)
    Note over H: nothing persisted — the saga compensates (release + reject)
```

> **Idempotent:** a redelivered `MakePayment` finds the existing account and **re-publishes** `OrderPaymentScheduled` (no duplicate account), so a lost reply still reaches the saga.

---

## Integration Events

| Direction | Contract | Meaning |
|-----------|----------|---------|
| **In** | `Order.Saga.MakePayment` | Open a finance account and schedule payment for the order total. |
| **Out** | `Order.Saga.OrderPaymentScheduled` | Account created + schedule calculated; the saga may advance (`AccountId`, `PaymentCount`). |
| **Out** | `Order.Saga.OrderPaymentScheduleFailed` | Could not schedule (invalid total/frequency); the saga compensates (`Reason`). |

Contracts live in `Shared/src/EShop.Shared.Contracts/Services/Order/Saga/`.

---

## Data Model

| Table | One row per | Key constraint |
|-------|------------|----------------|
| `Accounts` | order × tenant | PK `Id`; `UNIQUE(tenant_id, order_id)` |
| `Payments` | scheduled payment | PK `Id`; FK `AccountId`; `UNIQUE(account_id, sequence)` |
| `InboxMessages` | processed message | Scaffolded — idempotency currently via `FindByOrderIdAsync` + the unique constraint |

Migrations are applied on startup via `DbInitializer`.

---

## API

| Method | Path | Response | Note |
|--------|------|----------|------|
| `POST` | `/api/v1/accounts/{orderId}` | `200 OK` account + payments / `404` | Read endpoint to inspect an account (defined in `AccountEndpoints`). |

> The endpoint is **defined but not yet mapped** into the request pipeline — `Startup.Configure` does not call `MapAccountEndpoints()` yet (see Roadmap G3). Finance is driven primarily by the `MakePayment` consumer, not HTTP.

---

## Configuration

| Key | Source | Purpose |
|-----|--------|---------|
| `ConnectionStrings:financeDatabase` / `DefaultConnection` | Aspire / appsettings | PostgreSQL connection |
| `MasstransitConfiguration` / `rabbitmq` | appsettings | RabbitMQ connection |
| `MessageBusOptions` | appsettings | Consumer retry policy |

Send-topology stamps `OrderId` as the MassTransit `CorrelationId` for `OrderPaymentScheduled` / `OrderPaymentScheduleFailed`.

---

## Tests

`Finance/tests/EShop.Finance.Tests` (xUnit + FluentAssertions + Moq) — 29 tests:

- `PaymentScheduleCalculatorTests` — frequency counts, even split, remainder absorption, due-date advance, invalid inputs.
- `PaymentScheduleStrategyTests` — factory resolves the right strategy per frequency, unknown frequency throws, a strategy builds its schedule independently.
- `AccountTests` — schedule generation, state transitions, completion, payment idempotency (domain, ready for the booking ticket).
- `CreateAccountCommandHandlerTests` — replies `OrderPaymentScheduled` on success, `OrderPaymentScheduleFailed` on invalid total, idempotent re-reply for an existing account.

```bash
dotnet test Finance/tests/EShop.Finance.Tests
```

---

## Roadmap

### Gap Analysis

| # | Gap | Status |
|---|-----|--------|
| G1 | **Booking deferred.** Pushing payments to a tenant's external accounting provider (`GenericHttp` provider), recording collected payments (`PaymentReceived` → `RecordPayment` → `Completed`). The domain `BookPayment`/`RecordPayment` exist and are unit-tested, but no application/infrastructure orchestration drives them. | Open |
| G2 | **`Account.Fail` is never orchestrated.** `AccountStatus.Failed` is reachable only via the domain method; no consumer/handler calls it today. | Open |
| G3 | **Account read endpoint not wired.** `AccountEndpoints.MapAccountEndpoints()` is defined but not called in `Startup.Configure`. | Open |
| G4 | **`InboxMessages` scaffolded but unused.** Idempotency relies on `FindByOrderIdAsync` + `UNIQUE(tenant_id, order_id)`. | Open |
| G5 | Strategy-based schedule calculation + aggregate-owned integrity assertion + saga reply flow. | **Resolved** |

### Suggested Implementation Order

1. Booking ticket — add the `GenericHttp` accounting provider + `BookPayment` orchestration, then `PaymentReceived` → `RecordPayment` → `OrderPaymentCompleted` — closes G1/G2.
2. Map `AccountEndpoints` in `Startup.Configure` (and add list/by-account routes) — closes G3.
3. Adopt the shared inbox for the consumer if at-least-once redelivery becomes a concern — closes G4.

---

## References

| Resource | Description |
|----------|-------------|
| [Order Service README](../../../Order/src/EShop.Order.API/README.md) | The Process Manager that issues `MakePayment` and consumes the reply events |
| [Inventory Service README](../../../Inventory/src/EShop.Inventory.API/README.md) | The other downstream of the saga (reserve / confirm / release) |
| [Domain-Driven Design](https://www.domainlanguage.com/ddd/) | Eric Evans — Original DDD book |
| [Event Storming](https://www.eventstorming.com/) | Alberto Brandolini — Discovery technique |
