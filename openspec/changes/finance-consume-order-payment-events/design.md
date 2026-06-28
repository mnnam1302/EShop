## Context

EShop's Order saga (`OrderSagaStateMachine`) drives `ReservingInventory → ProcessingPayment` but has no payment-side actor: nothing collects money or records the order financially, and the saga has no path out of `ProcessingPayment`. The **Finance** service exists only as scaffolding (`Program.cs` + assembly references). This change gives Finance its first behavior.

The design is informed by two patterns in the sibling `seamless-apps-dotnet` (Komodo) repo, deliberately distilled to EShop's far smaller scope:

- **Payment frequency / instalments** — Komodo's `PaymentFrequency` constants (`Monthly`, `Quarterly`, `Annually`, `OneOff`, …) and instalment-based billing. We adopt the frequency vocabulary and the idea of splitting a total into dated instalments, but not Komodo's full premium/reconciliation engine.
- **Generic HTTP accounting provider** — Komodo's `HttpIntegrationProvider` (`Name => "GenericHttp"`) implementing a common `IAccountingIntegrationProvider`, plus MotorRegistry's per-tenant `ProviderConfiguration` (URL + auth type + request/response templates). We adopt the named-provider abstraction and per-tenant templated HTTP config, but not Komodo's billing/checkout/reinsurance surface.

EShop conventions this change must follow (from existing services):
- Clean Architecture layering `API → Infrastructure → Application → Domain`; file-scoped namespaces; primary constructors for DI.
- Integration events derive from `IntegrationEvent` (carries `TenantId`/`ActionUserId`/`ActionUserType`) and live in `EShop.Shared.Contracts/Services/<Service>/`.
- MassTransit consumers translate messages to MediatR commands (see `MakeReservationConsumer`).
- Idempotency via the shared inbox (`InboxMessage`, `inbox_messages` table, `AddInboxMessageEntity` model builder extension).
- Multi-tenancy via `IScoped` + EF Core global query filters; never share a `DbContext` across tenants.
- Domain invariants via `Specification` + `DomainException`; handlers return `Result`/`Result<T>`.

## Goals / Non-Goals

**Goals:**
- Finance owns the post-placement payment lifecycle for an order: account creation, schedule generation, provider booking, payment recording, and lifecycle events back to the saga.
- A `payment-schedule` calculator that splits a total into instalments by `OneOff/Monthly/Quarterly/Annually` with exact-sum rounding and correct due dates — unit-testable in isolation.
- A pluggable `IAccountingIntegrationProvider` with a per-tenant configurable `GenericHttp` implementation; new third-parties onboarded by config only.
- Retry-safe (idempotent) external booking using a deterministic idempotency key.
- BDD/integration coverage of the three capabilities and a service README.

**Non-Goals:**
- No real payment-gateway/checkout UI, card handling, or PCI scope — payment receipt arrives as an integration event.
- No reconciliation, refunds, partial payments, dunning, proration, or mid-term adjustments.
- No change to the Inventory service. Order service change is limited to consuming the two new Finance events to leave `ProcessingPayment`.
- No replacement of Komodo's billing engine; we implement the minimum that satisfies the specs.

## Decisions

### D1: One aggregate — `Account` — owns the order's instalments
The `Account` aggregate root (keyed by `(TenantId, OrderId)`, `IScoped`, `IAggregateRoot`) holds the order total/currency, `PaymentFrequency`, status (`AwaitingSchedule → Scheduled → Completed/Failed`), and a collection of `Instalment` child entities. Schedule generation, booking, and payment recording are behavior methods on `Account` so all invariants live in one aggregate — honoring "one command touches one aggregate." `Instalment` carries its amount, due date, state (`Pending → Booked → Paid|Failed`), and external booking reference.

*Alternative considered:* separate `PaymentSchedule` aggregate. Rejected — it would force cross-aggregate coordination for every booking/payment with no real consistency boundary between an account and its own instalments.

### D2: Payment schedule is a pure domain service / value calculation
`PaymentScheduleCalculator` (domain) takes `(total, currency, frequency, startDate)` and returns instalment `(amount, dueDate)` tuples. Even split at the currency's minor unit; the remainder is added to the **final** instalment so amounts always sum to the total (spec `payment-schedule`). Frequency → `(count, interval)`: OneOff/Annually → relevant count, Monthly → `AddMonths(1)`, Quarterly → `AddMonths(3)`. Pure and deterministic → exhaustively unit-testable without infrastructure.

*Alternative:* push rounding to the first instalment. Rejected — absorbing in the last instalment matches the spec example (33.33, 33.33, 33.34) and common billing practice.

### D3: `IAccountingIntegrationProvider` resolved by name; `GenericHttp` is the only implementation now
Abstraction in **Application** (`Name`, `Task<BookingResult> BookInstalment(BookingContext, ct)`). A `Func<string, IAccountingIntegrationProvider>` / keyed-DI resolver selects the provider named in the tenant's config. `GenericHttp` lives in **Infrastructure** and uses a typed `HttpClient`. Adding a bespoke provider later means registering another implementation — no caller changes. Matches Komodo's `Name => "GenericHttp"` shape.

### D4: Per-tenant `GenericHttp` config via options, templated request/response
`GenericHttpProviderOptions` keyed by `TenantId`: `BaseUrl`, `BookingPath`, `AuthenticationType` (`None | BasicAuthentication | BearerToken`), credentials, and `RequestTemplate`/`ResponseTemplate`. Templates are simple `{{placeholder}}` token substitution over booking fields (amount, currency, dueDate, idempotencyKey, orderId, instalmentId) — no heavyweight templating dependency for this scope. Config source is `IConfiguration`/options bound from appsettings (secrets injected via env at deploy, none committed). Onboarding a tenant = add a config section.

*Alternative:* store provider config in the database with a CRUD API (as Komodo does via `AccountingCompany`/`BillingProvider`). Deferred as a non-goal; options-based config is the minimum that satisfies the per-tenant spec and keeps the change surgical.

### D5: Idempotency at two levels
- **Inbound** (events): shared inbox pattern — consumer checks `inbox_messages` keyed by `(ConsumerId, MessageId)` before dispatching the command; account creation also guarded by a `UNIQUE(tenant_id, order_id)` constraint, instalment payment guarded by instalment state.
- **Outbound** (provider booking): deterministic idempotency key = stable hash/string of `(TenantId, AccountId, InstalmentId)` sent as a header (e.g. `Idempotency-Key`) and persisted with the booking reference. Re-booking an already-`Booked` instalment short-circuits to the stored reference. Aligns with the `idempotent-external-integration` design.

### D6: Flow & event contracts
1. Order side publishes an "order awaiting payment" trigger carrying `OrderId`, total, currency, buyer, optional `PaymentFrequency`. (New contract under `Services/Order/` or `Services/Finance/`; chosen: a Finance-owned `RecordOrderPayment`-style trigger consumed from the order event. Implementation will reuse `OrderCreated` if it already carries enough, else add `OrderAwaitingPayment`.)
2. `OrderAwaitingPaymentConsumer` → `CreateFinanceAccountCommand` → `Account` created + schedule generated.
3. A booking step books each due instalment via the resolved provider (`Booked` + external ref).
4. `PaymentReceivedConsumer` → `RecordInstalmentPaymentCommand` → instalment `Paid`; when all paid → `Account.Completed` → publish `OrderPaymentCompleted`. Terminal failure → `OrderPaymentFailed`.
5. Order saga consumes `OrderPaymentCompleted`/`OrderPaymentFailed` to confirm/release the inventory reservation (additive saga wiring).

### D7: Persistence
`FinanceDbContext` (EF Core + PostgreSQL) with `Accounts`, `Instalments`, `inbox_messages` (via `AddInboxMessageEntity`). Scoped per tenant. One EF migration. Registered in `EShop.AppHost` as a new resource with its own database, mirroring other services.

## Risks / Trade-offs

- **Booking placement (when to call the provider)** → Booking on schedule creation couples account creation latency to an external call. Mitigation: book instalments in a dedicated command/handler step that is retry-safe (D5), so a provider outage doesn't lose the account; the booking can be retried. For this change, booking is triggered immediately after scheduling but isolated so failure only blocks `Booked`, not account creation.
- **Template injection / malformed templates** → token substitution over tenant-controlled templates could produce invalid JSON or leak fields. Mitigation: restrict to a fixed, documented placeholder allow-list; serialize values safely; validate config at startup.
- **Idempotency key collisions across redeploys** → key derived purely from stable IDs `(TenantId, AccountId, InstalmentId)`, never from timestamps, so retries are stable. Trade-off: re-booking after a legitimate external cancellation needs explicit handling — out of scope (non-goal).
- **Rounding correctness** → final-instalment absorption is covered by explicit unit tests including the 100.00/3 case; without them, off-by-a-cent bugs are easy.
- **Scope creep toward Komodo's full engine** → kept in check by the Non-Goals; only the three specs are implemented.
- **Order-side contract ambiguity** → whether to extend `OrderCreated` or add `OrderAwaitingPayment` is resolved during implementation against the actual saga; the Finance side is agnostic to which trigger contract is used as long as it carries total/currency/frequency.

## Migration Plan

1. Add Shared.Contracts event/command types (additive — no consumers break).
2. Build Finance Domain → Application → Infrastructure → API; add EF migration; register Finance + database in `EShop.AppHost`.
3. Wire Order saga to consume the two new Finance events (additive states/handlers; existing transitions unchanged).
4. Deploy Finance and the updated Order service together. Rollback: Finance is a new service with no upstream consumers except the additive Order-saga handlers; disabling the Finance consumers and reverting the Order-saga wiring restores prior behavior. The new DB/tables are isolated and can be dropped.

## Open Questions

- Should provider config eventually move to the database with a management API (Komodo-style `BillingProvider`)? Deferred (D4 non-goal) — revisit if tenants need self-service onboarding.
- Should booking be synchronous within scheduling or a separate scheduled/queued step? Current decision: separate retry-safe step triggered post-scheduling; revisit if provider latency hurts throughput.
- Exact order-side trigger contract (`OrderCreated` vs new `OrderAwaitingPayment`) — finalized in implementation against the saga's `ProcessingPayment` entry.
