## Context

The Finance service already owns the payment lifecycle of an order: `Account` aggregate + `PaymentScheduleCalculator` (Strategy per `PaymentFrequency`) generate a payment schedule and reply to the Order saga (`OrderPaymentScheduled` / `OrderPaymentScheduleFailed`). The `Account` aggregate already exposes `BookPayment(paymentId, externalReference)` → `PaymentBooked` and `RecordPayment(...)` → `PaymentPaid`, but nothing calls them — booking to a real external system was deferred at service introduction.

This change fills that gap. The design is modelled on the sibling `seamless-apps-dotnet` (Komodo) `Finance` service's `GenericHttp` integration, which the user designated as the reference. Key difference in stacks: **Komodo Finance is EventFlow + JSON:API controllers; EShop Finance is Clean Architecture with MediatR command/query handlers, EF Core, MassTransit/RabbitMQ, and Minimal APIs**. We port the *patterns*, not the EventFlow plumbing.

Constraints:
- Multi-tenant: every provider artefact is `IScoped` (`TenantId`/`Scope`) and isolated by EF global query filters. Never share a `DbContext` across tenants.
- At-least-once delivery: booking side-effects must be idempotent (existing `IdempotentConsumer<T>` + `inbox_messages`).
- Follow the Four Principles — especially Simplicity First: port a trimmed subset, not Komodo's full surface.

## Goals / Non-Goals

**Goals:**
- Book an account's scheduled payments into the tenant's own accounting system with **zero code per tenant** — a new REST provider is onboarded by writing YAML + connection details.
- One authentication stack covering OAuth2 client-credentials (with per-tenant token reuse), Basic, and NoAuth, selected by configuration.
- Exactly-once *effect* booking under retries/redelivery (find-then-create + the payment's recorded external reference).
- Keep order acceptance fast: booking runs asynchronously, decoupled from the saga schedule reply.
- A management surface to register a provider config, store credentials, and test the connection before use.

**Non-Goals:**
- Reconciliation, multi-company-per-tenant routing driven by payment method, claims/reinsurance/checkout flows, batch booking confirmation — all present in Komodo, all excluded here.
- The **billing/collection** side (charging the customer via a gateway and `RecordPayment`) — deferred to a later change; this change books to the *accounting* provider only. The `IAccountingIntegrationProvider` abstraction is shaped so collection can be added later without rework.
- Response-template V2 namespaces and binary/PDF retrieval.
- Replacing the existing saga schedule reply. Booking is downstream of it.

## Decisions

### D1 — Named-strategy provider registry (not a switch)
`IAccountingIntegrationProvider` exposes `string Name` (`"GenericHttp"`, `"None"`). Implementations are registered in DI as a set; `AccountingIntegrationProviderFactory.Create(tenantId)` reads the tenant's configured provider type and resolves `providers.Single(p => p.Name == type)`. **Why:** open/closed — future bespoke providers (a native Stripe/QuickBooks client) drop in without touching call sites; mirrors Komodo. **Alternative rejected:** a `switch` on provider type (couples every caller to the provider set).

### D2 — Configuration-as-data via YAML + Handlebars (the core bet)
Provider behaviour lives in a `FinanceConfiguration` deserialized from YAML: `dateFormat`, `overrides` (behaviour flags), `triggers → actions → requests`, where each `RequestConfiguration` holds `UrlTemplate`, `Method`, `RequestTemplate`, `ResponseTemplate` as Handlebars templates. The `HttpIntegrationClient.ExecuteRequest<TEntity,TResult>` renders URL/body from a template data model, sends via a resilient `HttpClient`, then **re-shapes the provider response through `ResponseTemplate` into a strongly-typed internal result** — so heterogeneous provider payloads normalise to one model. Provider status vocabulary maps through `overrides` (`"CLOSED" → "PAID"`).
**Why:** onboarding = YAML, not a deploy — the whole point of the SaaS requirement. **Alternatives rejected:** (a) one C# client per provider (doesn't scale to N tenants); (b) a rules/DSL of our own (Handlebars + YAML is proven in the reference and off-the-shelf: `YamlDotNet` + `Handlebars.Net`).

### D3 — Auth: connection-details dict → typed options → scheme-selected provider
Per-tenant `ConnectionDetails` is a `Dictionary<string,string?>` (baseUrl, scheme, clientId, clientSecret, scope, tokenEndpoint, username, password). `AuthenticationOptions.Create(dict)` parses/validates by `Scheme` (default OAuth, case-insensitive keys, raw dict retained for provider-specific keys). `IAuthenticationProvider` implementations {`OAuth`, `Basic`, `NoAuth`} are selected by scheme and `ApplyAuthentication(request)`.
**OAuth token session cache:** the access token is persisted per tenant in an `IntegrationProviderSession` row and reused until ~3 minutes before its JWT `ValidTo`; only re-fetched when expired or the provider changed. **Why:** avoids hammering token endpoints and shares tokens across requests/nodes. **Alternative rejected:** in-memory-only cache (lost per node/restart; the reference persists it).

### D4 — Booking pipeline: async, idempotent, find-then-create
After payments are scheduled, Finance publishes a `PaymentsScheduled` integration event. A `BookAccountPaymentsConsumer` (an `IdempotentConsumer<T>` keyed on account id) resolves the tenant's provider and, per payment: (1) short-circuit if the `Payment` already has an `ExternalBookingReference`; (2) **Find** at the provider (search request); (3) **Create** only if not found; (4) call `Account.BookPayment(paymentId, externalRef)`, which records the reference on the `Payment` and is itself idempotent (`Payment.MarkBooked` no-ops when already booked). Transient failures publish `PaymentBookingFailed` and are safe to retry — the ledger converges.
**Why the `Payment` row is the idempotency record (no separate map):** `Payment` already carries `ExternalBookingReference` and an idempotent `MarkBooked`, so a dedicated `PaymentBookingReference` table would be redundant — confirmed during Phase 1. **Why async & decoupled from the saga:** external-provider latency must not stall order acceptance; the saga already completes on *schedule*. **Why layered idempotency:** the CAS lesson from stock-deduction — the inbox dedupes redelivery, `Payment.ExternalBookingReference` short-circuits re-booking, and find-before-create guards against a crash *between* external create and local commit (the reference may exist remotely but not locally). **Alternative rejected:** booking synchronously inside the schedule handler (couples order latency to ERP latency; no natural retry).

### D5 — Secrets stored encrypted in the Finance database (never in a shared cache)
`AccountingCompany` (`IScoped`) holds the provider type + the YAML behaviour config plus an `EncryptedConnectionDetails` column (a JSON connection-details document). **Tenant secrets (API keys, `clientSecret`, `password`) live only in that encrypted column in the Finance Postgres database — never in the YAML and never in Redis/`EShop.Shared.Cache`.** The OAuth access-token cache (`IntegrationProviderSession.SessionToken`) is likewise a Finance Postgres table, encrypted at rest (this also matches the Komodo reference, which persists its session token in its own DbContext, not a cache). Encryption is applied by an infrastructure EF `ValueConverter` / `IConnectionDetailsStore` (Phase 3), keyed from application configuration. Credential input follows a separate "provide credentials / save credentials" step, and credentials are never returned on reads. **Why:** least privilege, transactional consistency with the config (one store, one write), and safe config export — the YAML can be reviewed/versioned without leaking secrets. **Alternative rejected:** storing connection details/token in Redis (adds a second store to keep consistent and places secrets in a shared cache — explicitly rejected on review).

### D6 — Resilience
The integration `HttpClient` is a named client with a Polly pipeline (retry on transient 5xx/timeout + circuit breaker) and a configurable timeout, matching the reference `ServicesConfiguration`. **Why:** third-party APIs are flaky; combined with D4's retry this gives converging, non-blocking booking.

### D7 — Feature-flag / config gating
Booking is gated so a tenant with no provider configured resolves to `None` (no-op) and the flow is inert. **Why:** additive rollout; existing tenants are unaffected until they register a provider.

### D8 — `AccountingCompany` provisioned on `TenantCreated`
Each tenant's accounting configuration is a domain aggregate named `AccountingCompany` (matching the reference's domain language). Finance consumes the existing `TenantCreated` integration event (as Authorization already does) and seeds exactly one `AccountingCompany` per tenant with `ProviderType = "None"`; the management API later *configures* it (provider type + YAML + credentials). Seeding is idempotent (no-op if one exists). **Why:** every tenant has a well-known, tenant-scoped config row from day one; configuration is a later, separate act. **Scope:** one company per tenant for now — multi-company-per-tenant routing stays a Non-Goal.

### D9 — What `Book` books: one scheduled payment
`IAccountingIntegrationProvider.Book(PaymentBookingContext ctx, CancellationToken ct)` records **one scheduled `Payment`** (an order instalment) in the tenant's accounting system and returns a `PaymentBookingResult { ExternalReference, Status? }`. `PaymentBookingContext` carries the tenant + account/order identifiers + payment fields (amount, currency, due date) and **doubles as the Handlebars template data model** the YAML request templates bind to. The `BookAccountPaymentsConsumer` iterates the account's `Pending` payments and calls `Book` per payment, then `Account.BookPayment(paymentId, result.ExternalReference)`. **Why per-payment (not batch):** clean failure isolation (one payment's failure doesn't abort the rest) and idempotency keyed on `Payment.ExternalBookingReference`. **Alternative rejected:** a single batch `Book(account)` call (the reference does this for reconciliation reasons EShop doesn't have).

## Risks / Trade-offs

- **Secret leakage via YAML/templates** → Enforce that credentials live only in the encrypted `EncryptedConnectionDetails` column in the Finance DB (D5), never in YAML or a shared cache; templates are data-only (no host-touching Handlebars helpers); redact request/response bodies in logs (the reference redacts sensitive keys).
- **Handlebars template misconfiguration by a tenant** (bad URL/body) → Fail the individual booking, publish `PaymentBookingFailed`, log redacted request; provide a `TestConnection` endpoint to validate at setup. Never let one payment's bad template abort the whole account run.
- **Idempotency gap between remote create and local commit** → Find-then-create keyed on a stable reference we send (e.g. payment id) so a re-run finds the prior create, backed by `Payment.ExternalBookingReference` + idempotent `MarkBooked` (D4).
- **Token cache race across nodes** (two nodes refresh simultaneously) → Acceptable: worst case is a redundant token fetch; last write wins on the session row. Not worth distributed locking at this scale.
- **New dependencies** (`YamlDotNet`, `Handlebars.Net`, `Polly`, OAuth token client) → All mature, widely used; isolated to the Finance Infrastructure/Application layers.
- **Scope creep toward the full Komodo surface** → Explicit Non-Goals; `IAccountingIntegrationProvider` kept minimal (book + test) so we don't pre-build reconciliation/checkout.

## Migration Plan

1. Additive EF Core migration adding two tables: `AccountingCompany` (`IScoped`, one row per tenant) and `IntegrationProviderSession` (per-tenant OAuth token cache). No changes to existing `Account`/`Payment` tables — booking idempotency reuses the existing `Payment.ExternalBookingReference`. **The migration is generated and applied manually by the maintainer** (not automated by this change).
2. Ship provider stack + consumer behind D7 gating; with no provider configured, behaviour is unchanged (resolves to `None`).
3. Register a provider config for a test tenant, validate via `TestConnection`, then exercise booking end-to-end against a fake HTTP endpoint (BDD).
4. **Rollback:** stop/unbind the `BookAccountPaymentsConsumer` (booking simply stops; scheduling and the saga are unaffected). Tables are additive and can remain; drop only if fully reverting.

## Resolved Decisions

- **Secret storage (confirmed on review):** connection details/credentials and the OAuth token are stored **encrypted at rest in the Finance Postgres database** (`AccountingProviderConfig.EncryptedConnectionDetails`, `IntegrationProviderSession.SessionToken`), applied by an infrastructure EF `ValueConverter`/`IConnectionDetailsStore` with a key from application configuration. **Not Redis / `EShop.Shared.Cache`.**
- **Booking trigger:** a new internal `PaymentsScheduled` integration event, so booking is independent of the `OrderPaymentScheduled` saga reply.
- **Idempotency store:** reuse the existing `Payment.ExternalBookingReference` + idempotent `Payment.MarkBooked` (plus the `IdempotentConsumer` inbox for message dedupe). No separate `PaymentBookingReference` table.

## Open Questions

- None outstanding.
