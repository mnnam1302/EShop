## 0. Confirmed decisions (see design.md → Resolved Decisions)

- [x] 0.1 Secrets stored **encrypted at rest in the Finance Postgres DB** (`AccountingCompany.EncryptedConnectionDetails`, `IntegrationProviderSession.SessionToken`) — **not Redis/`EShop.Shared.Cache`**
- [x] 0.2 Booking trigger = new internal `PaymentsScheduled` integration event, independent of the `OrderPaymentScheduled` saga reply
- [x] 0.3 Idempotency = reuse existing `Payment.ExternalBookingReference` + idempotent `Payment.MarkBooked` (+ `IdempotentConsumer` inbox); **no separate `PaymentBookingReference` table**

## 1. Domain & persistence

- [x] 1.1 Add `AccountingCompany` aggregate (`IScoped`: `TenantId`/`Scope`, `IDateTracking`) holding `ProviderType` (default `"None"`), `YamlConfiguration`, and `EncryptedConnectionDetails` (encryption wired in 3.5); unique index on `TenantId` (one company per tenant)
- [x] 1.2 Add `IntegrationProviderSession` entity (per-tenant OAuth token cache: `TenantId` PK, `SessionToken`)
- [x] 1.3 Register both entities as DbSets on `FinanceDbContext` with entity type configurations (tenant isolation via the existing `IScoped` mechanism)
- [x] 1.4 Seed the tenant's `AccountingCompany` on tenant creation: `CreateAccountingCompanyCommand` + handler (idempotent — no-op if one already exists) and a `TenantCreatedConsumer` (mirrors Authorization's) that dispatches it → verify: unit test that consuming `TenantCreated` creates exactly one `None` company for the tenant
- [x] 1.5 Build entities/configs/DbSets so the project compiles; **the EF migration is generated and applied manually by the maintainer** (not automated here) → verify: `dotnet build` succeeds (0 errors) and the model is registered

## 2. Provider abstraction & factory

- [x] 2.1 Define `IAccountingIntegrationProvider` in Application: `string Name`, `Task<PaymentBookingResult> Book(PaymentBookingContext ctx, CancellationToken ct)` (books one scheduled payment; `PaymentBookingContext` doubles as the template data model), `Task<bool> TestConnection(IReadOnlyDictionary<string,string?> connectionDetails, CancellationToken ct)`; add `PaymentBookingContext` / `PaymentBookingResult` DTOs
- [x] 2.2 Implement `NoneAccountingIntegrationProvider` (`Name => "None"`, no-op book/test)
- [x] 2.3 Implement `AccountingIntegrationProviderFactory` resolving by tenant's `ProviderType` via `Single(p => p.Name == type)`, defaulting to `None`, and throwing on unknown type
- [x] 2.4 Register providers + factory in DI → verify: unit tests for resolve-configured, resolve-unconfigured→None, unknown-type→error (covers `finance-provider-configuration` resolution scenarios) — 3 tests green

## 3. Authentication stack

- [x] 3.1 Implement `AuthenticationOptions.Create(connectionDetails)` (default scheme OAuth, case-insensitive keys, retain raw dict, validate per scheme)
- [x] 3.2 Define `IAuthenticationProvider` (+ resolver) and implement `BasicAuthenticationProvider` and `NoAuthAuthenticationProvider`
- [x] 3.3 Implement `OAuthAuthenticationProvider` (client-credentials token request; body vs. Basic-header credential modes)
- [x] 3.4 Add per-tenant token caching in the OAuth provider using `IntegrationProviderSession` (+ `ExpiresAtUtc`), reusing until ~3 min before expiry, refreshing on expiry
- [x] 3.5 Implement `IConnectionDetailsStore` + `IProviderSessionStore` + an AES `IFieldEncryptor` (key from app config) so `EncryptedConnectionDetails` and `IntegrationProviderSession.SessionToken` are ciphertext at rest in Postgres — encryption applied in the stores (not an EF `ValueConverter`, to keep the DbContext free of runtime services) → verify: unit tests for scheme selection, Basic/OAuth header application, missing-required-OAuth rejected, reuse-valid-token, refresh-expired-token, round-trip encrypt/decrypt — 6 tests green

## 4. YAML configuration & HTTP client

- [x] 4.1 Add `YamlDotNet`, `Handlebars.Net`, and `Microsoft.Extensions.Http.Resilience` to the Finance projects (CPM)
- [x] 4.2 Implement the `FinanceConfiguration` model (`dateFormat`, `triggers → actions → requests`, `RequestConfiguration`) — overrides trimmed for the MVP
- [x] 4.3 Implement `ProviderConfigurationParser.Parse(yaml)` (blank → empty config; malformed → descriptive error)
- [x] 4.4 Implement request resolution by `(trigger, action)` with case-insensitive matching returning null when absent
- [x] 4.5 Implement `HandlebarsHelper` for URL/request/response template compilation
- [x] 4.6 Implement the resilient named `HttpClient` (`AddStandardResilienceHandler`: timeout + retry + circuit-breaker) in the Infrastructure DI extension
- [x] 4.7 Implement `HttpIntegrationClient.ExecuteRequest` (render URL/body, apply auth, send, shape response via `ResponseTemplate` → string map, null on not-found/no-content, surface `ServerCommunicationException` with status)
- [x] 4.8 Add secret redaction for logged request/response detail → verify: 8 tests for parse valid/blank/malformed, resolve request, render URL+body, response shaping, not-found→null, error-with-status — green

## 5. GenericHttp provider

- [x] 5.1 Implement `GenericHttpAccountingProvider` (`Name => "GenericHttp"`) with `TestConnection` delegating to `VerifyAuthentication` (non-verifiable scheme → success)
- [x] 5.2 Implement `Book(...)` resolving the tenant's config, building the template data model from a payment, and executing the configured find/create requests
- [x] 5.3 Build a `HttpAccountingContext` that resolves `RequestConfiguration`s (find/create) from the tenant `FinanceConfiguration` → verify: 3 provider tests (create-when-none, adopt-existing, test-connection) green

## 6. Booking pipeline

- [x] 6.1 Define/publish the `PaymentsScheduled` trigger (Shared.Contracts/Services/Finance) when an account's payments are scheduled (in `CreateAccountCommandHandler`, alongside the saga reply)
- [x] 6.2 Implement `BookAccountPaymentsConsumer` (`IConsumer<PaymentsScheduled>` → `BookAccountPaymentsCommand`); idempotency comes from `Payment.ExternalBookingReference` + throw-to-retry on partial failure (chosen over the inbox because the inbox's ack semantics would suppress retry of unbooked payments)
- [x] 6.3 Per payment: short-circuit when the `Payment` already has an `ExternalBookingReference`, else find-then-create at provider (idempotency via `Payment.MarkBooked`)
- [x] 6.4 On success, invoke `Account.BookPayment(paymentId, externalReference)` and persist → `PaymentBooked` raised
- [x] 6.5 On failure, publish `PaymentBookingFailed` per payment, keep it retry-eligible, and ensure one failure does not abort the remaining payments
- [x] 6.6 Ensure unconfigured tenant (`None`) performs no external call → verify: 4 handler tests for create-all, failure-isolation, none-no-op, already-booked-skip (covers `finance-payment-booking` scenarios)

## 7. Management API

- [x] 7.1 Add endpoint (`PUT api/v1/accounting-company`) to configure the tenant's existing `AccountingCompany` (set provider type + YAML)
- [x] 7.2 Add endpoint (`PUT api/v1/accounting-company/credentials`) to save connection credentials (stored encrypted, excluded from reads)
- [x] 7.3 Add a `TestConnection` endpoint (`POST api/v1/accounting-company/test-connection`) returning `{valid}` without booking
- [x] 7.4 Config read (`GET api/v1/accounting-company`) returns provider type + YAML + `hasConnectionDetails` only — never credential values; also wired `MapEndpoints()` into the pipeline (closes prior gap G3)

## 8. BDD tests — DEFERRED (unit tests cover the scenarios)

> The Finance test project is plain xUnit with no Reqnroll/TestServer infra. All `finance-*`
> spec scenarios are already covered by the 55 unit tests (auth schemes, token reuse/refresh, YAML
> parse/resolve, response shaping, find-then-create, adopt-existing, failure isolation, none-no-op,
> encryption round-trip, test-connection). Standing up Reqnroll + a TestServer host is a separate
> follow-up; deferred rather than added unverified.

- [ ] 8.1 Add Reqnroll + `EShop.Testing.IntegrationTest` host to the Finance test project (fake HTTP handler recording outgoing requests)
- [ ] 8.2 `.feature` for provider configuration + `TestConnection`
- [ ] 8.3 `.feature` for booking (load YAML, assert outgoing request + recorded external reference)
- [ ] 8.4 Scenarios for idempotent redelivery and failure isolation

## 9. Wiring & docs

- [x] 9.1 Register the full provider/auth/client/stores/consumer stack across the Finance Application + Infrastructure `ServiceCollectionExtensions` (done incrementally per phase)
- [x] 9.2 `financeDatabase` resource already present in `EShop.AppHost`; added `Finance:Encryption:Key` (dev) to `appsettings.Development.json` (prod supplies via env `Finance__Encryption__Key`)
- [x] 9.3 Documented the GenericHttp provider, YAML shape, auth, and booking flow in `Finance/src/EShop.Finance.API/README.md` (+ updated gap table G1/G3) → verify: full solution builds (0 errors), 55 Finance tests pass
