## 0. Scope update (2026-06-28) — booking split to a follow-up ticket

This change now ships only **create account → calculate & schedule payments → reply to the Order saga**.
The **booking** feature (push each payment to the tenant's external accounting provider + record collected
payments) moves to a separate ticket. Net effect on the tasks below:

- **Done & shipping:** create account + schedule (§2, §3.2, §3.5, §4.1–4.3, §5), Strategy-pattern calculation
  (§1.4), and the saga **reply flow** — `CreateAccountCommandHandler` publishes
  `Order.Saga.OrderPaymentScheduled` / `OrderPaymentScheduleFailed`, consumed via `MakePaymentConsumer`.
  Covered by `CreateAccountCommandHandlerTests` (replaces the old §7.5).
- **Removed / deferred to the booking ticket:** the GenericHttp provider stack (§3.1, §4.5–4.7, §7.4),
  `BookInstalmentsCommand` (§3.3), `RecordInstalmentPayment` + `PaymentReceived` consumer (§3.4, §4.4),
  and the `OrderPaymentCompleted`/`OrderPaymentFailed`/`PaymentReceived` contracts (§1.1, §1.3). The
  `Account`/`Payment` domain methods for booking/paying remain in place (unit-tested) ready for that ticket.
- **Still open (Order-side ticket):** §6 — the event-sourced `OrderSaga` *publishing* `MakePayment` and
  *consuming* the two reply events.

The checkboxes below reflect the original plan and are kept for history; this section is the source of truth.

---

## 1. Shared contracts

- [x] 1.1 Add `Services/Finance/OrderPaymentCompleted.cs` and `OrderPaymentFailed.cs` (derive `IntegrationEvent`, carry `OrderId`)
- [x] 1.2 Add the inbound saga-command contract `Services/Order/Saga/MakePayment.cs` (`IntegrationCommand`, carrying total, currency, buyer, optional `PaymentFrequency`) — consumed by Finance to open an account
- [x] 1.4 Refactor payment calculation to a Strategy pattern (`IPaymentScheduleStrategy` + per-frequency strategies + `PaymentScheduleStrategyFactory`), aggregate-owned schedule-integrity assertion; see `design-strategy-and-makepayment.md`
- [x] 1.3 Add `Services/Finance/PaymentReceived.cs` (carry `OrderId`/`AccountId`, `InstalmentId`, amount, paid-at)

## 2. Domain (`EShop.Finance.Domain`)

- [x] 2.1 Add `PaymentFrequency` enum/constants (`OneOff`, `Monthly`, `Quarterly`, `Annually`)
- [x] 2.2 Add `Instalment` entity with state (`Pending → Booked → Paid|Failed`), amount, due date, external booking reference, and guarded transition methods
- [x] 2.3 Add `Account` aggregate root (`IScoped`, `IAggregateRoot`) keyed by `(TenantId, OrderId)` with total/currency/frequency/status and instalments collection
- [x] 2.4 Implement `PaymentScheduleCalculator` (pure): even split at currency minor unit, remainder to final instalment, frequency→count/interval, due dates
- [x] 2.5 Add specifications + `DomainException` paths for invalid frequency, zero/negative total, illegal transitions
- [x] 2.6 Raise domain events for account created, scheduled, instalment booked/paid, account completed/failed

## 3. Application (`EShop.Finance.Application`)

- [x] 3.1 Define `IAccountingIntegrationProvider` (`Name`, `BookInstalment(BookingContext, ct)` → `BookingResult` with external ref) and a name-based provider resolver
- [x] 3.2 Add `CreateFinanceAccountCommand` + handler (create account, generate schedule, persist) returning `Result`
- [x] 3.3 Add `BookInstalmentsCommand` + handler (resolve provider, deterministic idempotency key, mark `Booked`/record failure) returning `Result`
- [x] 3.4 Add `RecordInstalmentPaymentCommand` + handler (mark `Paid`, complete account when all paid, publish `OrderPaymentCompleted`/`OrderPaymentFailed`)
- [x] 3.5 Add DI extension `AddFinanceApplication`

## 4. Infrastructure (`EShop.Finance.Infrastructure`)

- [x] 4.1 Add `FinanceDbContext` with `Accounts`, `Instalments`, inbox (`AddInboxMessageEntity`), tenant query filters, `UNIQUE(tenant_id, order_id)`
- [x] 4.2 Add EF Core entity configurations + initial migration
- [x] 4.3 Add repository for `Account`
- [x] 4.4 Add MassTransit consumers: `OrderAwaitingPaymentConsumer` → `CreateFinanceAccountCommand`; `PaymentReceivedConsumer` → `RecordInstalmentPaymentCommand` (idempotent via inbox)
- [x] 4.5 Implement `GenericHttpAccountingProvider` (`Name = "GenericHttp"`) with typed `HttpClient`, auth from config, `{{placeholder}}` request/response templating, `Idempotency-Key` header
- [x] 4.6 Add `GenericHttpProviderOptions` (per-tenant: base URL, path, auth type, credentials, templates) + startup validation
- [x] 4.7 Add DI extension `AddFinanceInfrastructure` (DbContext, consumers, provider registration/resolver, options binding)

## 5. API & host (`EShop.Finance.API`, `EShop.AppHost`)

- [x] 5.1 Wire `AddFinanceApplication`/`AddFinanceInfrastructure`, MassTransit, MediatR, Swagger in `Program.cs`
- [x] 5.2 Add a read endpoint to inspect an account + its instalments (verification aid)
- [ ] 5.3 Register Finance service + its PostgreSQL database in `EShop.AppHost`
- [x] 5.4 Apply migration on startup (match existing services' approach)

## 6. Order saga wiring

- [ ] 6.1 Consume `OrderPaymentCompleted` → confirm reservation (`ConfirmReservationCommand`) and advance saga out of `ProcessingPayment`
- [ ] 6.2 Consume `OrderPaymentFailed` → release reservation (`ReleaseReservationCommand`) and fail the saga

## 7. Tests (BDD + unit)

- [x] 7.1 Unit-test `PaymentScheduleCalculator`: even split, 100.00/3 remainder, due-date advance per frequency, invalid frequency, zero/negative total
- [x] 7.2 Unit-test `Account`/`Instalment` transitions (valid progression + illegal transition rejected)
- [ ] 7.3 BDD/integration: order event → account + schedule created; redelivery does not duplicate
- [x] 7.4 BDD/integration: GenericHttp booking is idempotent (retry sends same key, single reference); two tenants hit two endpoints; missing config fails fast
- [ ] 7.5 BDD/integration: payment received marks instalment paid; final payment completes account and publishes `OrderPaymentCompleted`; terminal failure publishes `OrderPaymentFailed`

## 8. Documentation

- [x] 8.1 Write `Finance/src/EShop.Finance.API/README.md` (purpose, flow diagram, frequencies, GenericHttp config, integration events, tables) following the Inventory README style
- [ ] 8.2 Note the new service + GenericHttp config in the root docs/CLAUDE.md service list if appropriate
