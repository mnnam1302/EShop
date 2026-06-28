## ADDED Requirements

### Requirement: Provider abstraction

The system SHALL define an `IAccountingIntegrationProvider` abstraction with a stable `Name` and an operation to book an instalment, returning a result that indicates success and carries the provider's external booking reference. Multiple provider implementations SHALL be registerable, and the system SHALL resolve the provider to use by name.

#### Scenario: Resolve provider by name

- **WHEN** a tenant's configuration selects provider `GenericHttp`
- **THEN** the booking pipeline uses the `GenericHttp` provider implementation

#### Scenario: Unknown provider name fails fast

- **WHEN** a tenant's configuration selects a provider name with no registered implementation
- **THEN** booking fails with a clear configuration error and the instalment is not marked `Booked`

### Requirement: Per-tenant Generic HTTP provider configuration

The `GenericHttp` provider SHALL be configurable per tenant without provider-specific code, using a configuration that specifies the base URL, authentication type (e.g. `None`, `BasicAuthentication`, `BearerToken`), credentials, and request/response templates for the booking call. Onboarding a new third-party SHALL require only configuration, not new deployed code.

#### Scenario: Two tenants, two endpoints

- **WHEN** tenant A configures `GenericHttp` with base URL `https://a.example` and tenant B with `https://b.example`
- **THEN** an instalment for tenant A is booked against `a.example` and an instalment for tenant B against `b.example`

#### Scenario: Missing configuration fails fast

- **WHEN** an instalment is booked for a tenant that has no `GenericHttp` configuration
- **THEN** booking fails with a configuration error rather than calling an undefined endpoint

#### Scenario: Authentication applied from configuration

- **WHEN** a tenant configures `BearerToken` authentication with a token
- **THEN** the outbound booking request carries the `Authorization: Bearer <token>` header

### Requirement: Idempotent, retry-safe booking

Booking an instalment SHALL be retry-safe: the provider SHALL send a deterministic idempotency key derived from `(TenantId, AccountId, InstalmentId)` so that retried or redelivered bookings of the same instalment do not create duplicate bookings at the third party. A successful booking SHALL persist the external reference; a re-book of an already-booked instalment SHALL return the existing reference without a second side-effect.

#### Scenario: Retry does not double-book

- **WHEN** the same instalment is booked twice (e.g. after a transient failure retry)
- **THEN** the same idempotency key is sent both times
- **AND** the instalment retains a single external booking reference

#### Scenario: Successful booking is recorded

- **WHEN** the provider returns success with an external reference
- **THEN** the instalment transitions to `Booked` and stores that reference

### Requirement: Booking failure handling

When the provider returns a failure or is unreachable, the instalment SHALL NOT transition to `Booked`, the failure reason SHALL be recorded, and the operation SHALL be retryable.

#### Scenario: Provider returns an error

- **WHEN** the provider responds with a non-success status
- **THEN** the instalment remains in its pre-booking state with the failure reason recorded
- **AND** the booking can be retried without manual cleanup
