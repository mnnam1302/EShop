## Why

The Order saga today stops at `ProcessingPayment` — once inventory is reserved, nothing collects money or records the financial side of the order. The newly scaffolded **Finance** service has no behavior yet. We need Finance to take ownership of what happens after an order is placed: turn the order total into a payment plan (paid up-front or spread over monthly/quarterly/annual instalments), book those instalments against the tenant's external accounting/payment provider, and feed payment outcomes back so the order can be confirmed or released. Each tenant uses a different third-party provider, so the integration must be configurable per tenant rather than hard-coded.

## What Changes

- The Finance service **consumes Order integration events** (order placed/awaiting payment) idempotently via the existing inbox pattern and creates a finance `Account` that owns the order's payment lifecycle.
- A finance `Account` **generates a payment schedule** of one or more `Instalment`s from the order total based on a chosen `PaymentFrequency` (`OneOff`, `Monthly`, `Quarterly`, `Annually`), assigning each instalment an amount and a due date. Rounding remainders are absorbed by the final instalment so instalments always sum to the total.
- Finance **books each due instalment to the tenant's external accounting provider** through an `IAccountingIntegrationProvider` abstraction. A built-in **`GenericHttp`** provider drives any third party via per-tenant configuration (base URL, authentication type, request/response templates) — no provider-specific code needed to onboard a new tenant. Booking is **idempotent/retry-safe** (a deterministic idempotency key per instalment prevents double-booking on redelivery).
- Finance **consumes Payment integration events** (payment received for an instalment) idempotently, marks the instalment `Paid`, and when every instalment is settled publishes `OrderPaymentCompleted`; on terminal payment failure it publishes `OrderPaymentFailed`. These let the Order saga move past `ProcessingPayment` (confirm or release the inventory reservation).
- New integration event contracts are added under `EShop.Shared.Contracts/Services/Finance/` (and the order-side trigger/payment contracts they depend on).

## Capabilities

### New Capabilities

- `order-payment-ingestion`: Idempotent consumption of Order and Payment integration events that drive a finance `Account`'s lifecycle, and the outbound `OrderPaymentCompleted` / `OrderPaymentFailed` events back to the saga.
- `payment-schedule`: Calculation of a payment schedule (instalment amounts + due dates) from an order total and a `PaymentFrequency` of OneOff / Monthly / Quarterly / Annually, with remainder handling and instalment state transitions (Pending → Booked → Paid / Failed).
- `accounting-provider-integration`: The `IAccountingIntegrationProvider` abstraction and the per-tenant configurable `GenericHttp` provider that books instalments to a third-party HTTP API, including retry-safe (idempotent) booking and provider resolution per tenant.

### Modified Capabilities

<!-- None — no existing accepted spec's requirements change. Order-saga consumption of the new Finance events is additive and tracked under order-payment-ingestion. -->

## Impact

- **New code**: `Finance/src/EShop.Finance.Domain` (Account aggregate, Instalment entity, PaymentFrequency/PaymentSchedule value objects, domain events, specifications), `EShop.Finance.Application` (commands/handlers, provider abstraction, schedule calculator), `EShop.Finance.Infrastructure` (EF Core `FinanceDbContext` + migration, MassTransit consumers, inbox, `GenericHttp` provider + HTTP client, per-tenant provider config), `EShop.Finance.API` (DI wiring, endpoints, Swagger).
- **Shared contracts**: new event/command types under `EShop.Shared.Contracts/Services/Finance/`; an order/payment trigger contract the Finance consumers subscribe to.
- **Order service**: adds consumption of `OrderPaymentCompleted` / `OrderPaymentFailed` to advance the saga out of `ProcessingPayment` (additive; existing saga states unchanged otherwise).
- **Dependencies**: MassTransit/RabbitMQ (existing), EF Core + PostgreSQL (existing), `HttpClient` (typed client) and a templating helper for the GenericHttp provider, `EShop.AppHost` registration of the Finance resource and its database.
- **Config/secrets**: per-tenant provider connection details (URLs, credentials) sourced from configuration; no secrets committed.
