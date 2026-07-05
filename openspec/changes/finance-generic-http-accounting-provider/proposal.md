## Why

EShop's Finance service currently *schedules* an order's payments (`Account` + `PaymentScheduleCalculator`) and replies to the Order saga, but it never books those payments into the tenant's real bookkeeping/collection system — that step was deliberately deferred when the Finance service was introduced. As a multi-tenant SaaS, every tenant runs its own accounting/billing back office (QuickBooks, Xero, Fortnox, a bespoke ERP, a payment gateway), each with different endpoints, field names, status vocabularies, and authentication. We need to book payments to those third parties **without shipping code per tenant** — onboarding a new provider must be a configuration act, not a deployment.

## What Changes

- Introduce a **per-tenant accounting provider registration**: tenants store a provider *type*, encrypted connection details (base URL, credentials), and a YAML behaviour configuration, scoped by `TenantId`.
- Introduce a **YAML-driven generic HTTP provider** (`GenericHttp`) that executes booking calls defined entirely as configuration: `triggers → actions → requests` with Handlebars URL/request/response templates, so a new REST accounting API is onboarded by writing YAML — no code, no deploy.
- Introduce a **pluggable authentication stack** driven by connection details: OAuth2 client-credentials (with a per-tenant access-token session cache honouring JWT expiry), Basic, and NoAuth, plus a connection-test capability used at setup time.
- Introduce an **idempotent payment-booking pipeline**: when an `Account`'s payments are scheduled, resolve the tenant's provider, **find-then-create** each booking at the external system, write the external reference back via `Account.BookPayment(...)`, and retry safely on failure (no duplicate bookings under at-least-once delivery).
- Add a minimal management API to upsert a tenant's provider configuration/credentials and to test the connection.
- No breaking changes: this is additive and completes the previously deferred booking follow-up. Booking runs asynchronously *after* the existing saga schedule reply, so external-provider latency does not stall order acceptance.

## Capabilities

### New Capabilities
- `finance-provider-configuration`: per-tenant registration and storage of an accounting provider (provider type, encrypted connection details, YAML behaviour config), named-provider selection/resolution, and connection testing.
- `finance-generic-http-provider`: the `GenericHttp` provider that renders YAML-defined request/response templates (Handlebars) into HTTP calls, applies the configured authentication scheme (OAuth2 client-credentials with per-tenant token cache, Basic, NoAuth), and normalises provider responses/status vocabulary into internal models.
- `finance-payment-booking`: the idempotent booking pipeline that books an account's scheduled payments to the resolved provider (find-then-create + reference map), records external references on the `Account`, and retries on transient failure.

### Modified Capabilities
<!-- None. The Finance service's existing scheduling behaviour is unchanged; this change is purely additive. -->

## Impact

- **New code (Finance service)**:
  - Domain: `AccountingCompany` aggregate (per-tenant, `IScoped`) seeded on `TenantCreated`; idempotency reuses the existing `Payment.ExternalBookingReference` (no separate map table).
  - Infrastructure: `IntegrationProviderSession` (per-tenant OAuth token cache), an EF field encryptor, a `TenantCreatedConsumer` (seeds the company), and EF configs. **The EF migration is generated/applied manually by the maintainer.**
  - Application: `IAccountingIntegrationProvider` + `GenericHttpAccountingProvider` (+ `None`) with `Book(PaymentBookingContext)` / `TestConnection`, `AccountingIntegrationProviderFactory` (named-strategy registry), `AuthenticationOptions` + `IAuthenticationProvider` {OAuth, Basic, NoAuth}, `FinanceConfiguration` YAML model + `ProviderConfigurationParser` (YamlDotNet) + `HandlebarsHelper` (Handlebars.Net) + a resilient `HttpIntegrationClient` (Polly).
  - Consumer/pipeline: a `BookAccountPaymentsConsumer` reacting to `PaymentsScheduled`, implemented as an `IdempotentConsumer<T>` keyed on account id.
  - API: management endpoints to configure the tenant's `AccountingCompany`, save credentials, and test connection.
- **Reuses existing shared infra**: `EShop.Shared.EventBus` (MassTransit, `IdempotentConsumer`, inbox), `EShop.Shared.Scoping` (tenant scoping); credential encryption is handled in the Finance DB (no shared cache).
- **New NuGet dependencies**: `YamlDotNet`, `Handlebars.Net`, `Polly` (resilience), an OAuth token client (e.g. `IdentityModel`).
- **Existing `Account` aggregate**: no schema change to booking behaviour — the change wires the already-present `BookPayment(paymentId, externalReference)` / `PaymentBooked` path.
- **Out of scope** (explicitly not ported from the Komodo reference): reconciliation, multi-company-per-tenant routing, claims/reinsurance/checkout, batch confirmation. Kept trimmed to EShop's needs per the Simplicity-First principle.
