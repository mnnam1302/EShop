# Order Service

> Accepts orders from buyers and coordinates stock reservation with the Inventory service via a **Process Manager** before confirming the order.

> Reference: [CQRS Journey — Chapter 6: Sagas and Process Managers](https://learn.microsoft.com/en-us/previous-versions/msp-n-p/jj591569(v=pandp.10))

---

## What This Service Does

```mermaid
graph LR
    classDef actor   fill:#fff9c4,color:#000,stroke:#f9a825
    classDef agg     fill:#f48fb1,color:#000,stroke:#c62828
    classDef pm      fill:#ce93d8,color:#000,stroke:#7b1fa2
    classDef inv     fill:#a5d6a7,color:#000,stroke:#2e7d32

    C([Customer]):::actor
    OA(Order Aggregate):::agg
    PM(Process Manager):::pm
    RA(Reservation Aggregate):::inv

    C -->|"PlaceOrder"| OA
    OA -->|"OrderCreated"| PM
    PM -->|"MakeReservation"| RA
    RA -->|"StocksReserved"| PM
    PM -->|"AcceptOrderCommand"| OA
    OA -->|"OrderAccepted"| C
```

**Two things this service owns:**

| | What it is |
|--|-----------|
| **Order aggregate** | The canonical purchase record — `Pending → Accepted / Rejected` |
| **Process Manager** (`OrderSaga`) | Listens to events, issues commands — no business logic |

---

## Event Storming — Place Order Flow

```mermaid
flowchart LR
    classDef event   fill:#ff9800,color:#fff,stroke:none
    classDef command fill:#42a5f5,color:#fff,stroke:none
    classDef agg     fill:#f48fb1,color:#000,stroke:#c62828
    classDef pm      fill:#ce93d8,color:#000,stroke:#7b1fa2
    classDef actor   fill:#fff9c4,color:#000,stroke:#f9a825

    Customer([Customer]):::actor
    PlaceOrder[PlaceOrderCommand]:::command
    OA(Order Aggregate):::agg
    OrderCreated([OrderCreated]):::event
    PM(Process Manager):::pm
    MakeReservation[MakeReservation]:::command
    RA(Reservation Aggregate):::agg
    StocksReserved([StocksReserved]):::event
    StocksNotReserved([StocksNotReserved]):::event
    AcceptOrder[AcceptOrderCommand]:::command
    RejectOrder[RejectOrderCommand]:::command
    ReleaseReservation[ReleaseReservationCommand]:::command
    OrderAccepted([OrderAccepted]):::event
    OrderRejected([OrderRejected]):::event

    Customer --> PlaceOrder --> OA --> OrderCreated --> PM
    PM --> MakeReservation --> RA
    RA --> StocksReserved --> PM --> AcceptOrder --> OA --> OrderAccepted
    RA --> StocksNotReserved --> PM --> RejectOrder --> OA --> OrderRejected
    StocksNotReserved --> PM --> ReleaseReservation --> RA
```

### Policies — When / Then Rules

| When this event | Then issue this command |
|----------------|------------------------|
| `OrderCreated` | `MakeReservation` to Inventory |
| `StocksReserved` | `AcceptOrderCommand` to Order |
| `StocksNotReserved` | `RejectOrderCommand` to Order |
| `StocksNotReserved` | `ReleaseReservationCommand` to Inventory (compensation) |
| `PaymentAccepted` *(follow-up)* | `ConfirmReservationCommand` to Inventory |
| `PaymentFailed` *(follow-up)* | `ReleaseReservationCommand` to Inventory |

---

## Domain Model

```mermaid
classDiagram
    class Order {
        Guid Id
        string BuyerId
        string Status
        string Description
        +CreateOrder(command)
        +Accept()
        +Reject(reason)
    }

    class OrderItem {
        Guid VariantId
        int Quantity
        decimal UnitPrice
        decimal Discount
    }

    class OrderStatus {
        <<enumeration>>
        Pending
        Accepted
        Rejected
    }

    Order "1" --> "1..*" OrderItem
    Order --> OrderStatus
```

---

## Order Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending : PlaceOrderCommand<br/>POST /api/v1/orders — 202 Accepted

    Pending --> Accepted : AcceptOrderCommand<br/>stock confirmed

    Pending --> Rejected : RejectOrderCommand<br/>stock failed

    Accepted --> [*]
    Rejected --> [*]
```

> Buyer gets `202 Accepted` immediately. `Accepted` / `Rejected` resolves asynchronously.

---

## Process Manager — How It Works

```mermaid
flowchart TB
    classDef event   fill:#ff9800,color:#fff,stroke:none
    classDef command fill:#42a5f5,color:#fff,stroke:none
    classDef pm      fill:#ce93d8,color:#000,stroke:#7b1fa2,stroke-width:2px

    subgraph PM["OrderSaga — Process Manager"]
        S1["AwaitingStockReservation"]
        S2["StocksAccepted"]
        S3["StocksRejected"]
    end

    E1([OrderCreated]):::event -->|"Apply"| S1
    S1 -->|"issues"| C1[MakeReservation]:::command --> INV["Inventory"]

    INV --> E2([StocksReserved]):::event --> S2
    INV --> E3([StocksNotReserved]):::event --> S3

    S2 -->|"issues"| C2[AcceptOrderCommand]:::command --> OA["Order Aggregate"]
    S3 -->|"issues"| C3[RejectOrderCommand]:::command --> OA
    S3 -->|"issues"| C4[ReleaseReservationCommand]:::command --> INV
```

| Question | Answer |
|----------|--------|
| What does a Process Manager do? | Listens to events, issues commands — no business logic, pure routing |
| Where is its state? | Event-sourced — rebuilt from `OrderSagaStartedEvent`, `StockReservedEvent`, `StockReservationFailedEvent` |
| How is it identified? | `OrderSagaId.FromOrderId(orderId)` — deterministic, no extra lookup |
| Duplicate `OrderCreated` delivered twice? | `IsNew` guard — second delivery is a no-op |

---

## Saga Lifecycle

```mermaid
stateDiagram-v2
    [*] --> AwaitingStockReservation : Event: OrderCreated<br/>Issues: MakeReservation

    AwaitingStockReservation --> StocksAccepted  : Event: StocksReserved<br/>Stores: ReservationId<br/>Issues: AcceptOrderCommand

    AwaitingStockReservation --> StocksRejected  : Event: StocksNotReserved<br/>Issues: RejectOrderCommand<br/>Issues: ReleaseReservationCommand

    StocksAccepted --> [*]
    StocksRejected --> [*]
```

---

## End-to-End Sequence

### Happy Path

```mermaid
sequenceDiagram
    autonumber
    participant C  as Customer
    participant OA as Order Aggregate
    participant PM as Process Manager
    participant RA as Reservation Aggregate

    C->>OA: PlaceOrderCommand
    OA-->>PM: OrderCreated
    OA-->>C: 202 Accepted

    PM->>RA: MakeReservation
    RA-->>PM: StocksReserved (ReservationId)

    PM->>OA: AcceptOrderCommand
    OA-->>C: OrderAccepted — Status = Accepted
```

### Compensation — Stock Failed

```mermaid
sequenceDiagram
    autonumber
    participant RA as Reservation Aggregate
    participant PM as Process Manager
    participant OA as Order Aggregate

    RA-->>PM: StocksNotReserved (reason)
    PM->>OA: RejectOrderCommand
    PM->>RA: ReleaseReservationCommand
    OA-->>OA: Status = Rejected
```

---

## Message Contracts

| Message | Kind | Sender | Receiver |
|---------|------|--------|----------|
| `OrderCreated` | Event | Order Aggregate | Process Manager |
| `MakeReservation` | Command | Process Manager | Inventory |
| `StocksReserved` | Event | Inventory | Process Manager |
| `StocksNotReserved` | Event | Inventory | Process Manager |
| `AcceptOrderCommand` | Command | Process Manager | Order Aggregate |
| `RejectOrderCommand` | Command | Process Manager | Order Aggregate |
| `ReleaseReservationCommand` | Command | Process Manager | Inventory |

---

## API

| Method | Path | Response | Note |
|--------|------|----------|------|
| `POST` | `/api/v1/orders` | `202 Accepted { orderId }` | Async — saga resolves after response |
