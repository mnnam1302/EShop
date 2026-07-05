## ADDED Requirements

### Requirement: YAML behaviour configuration parsing
The system SHALL parse a tenant's YAML behaviour configuration into a `FinanceConfiguration` composed of `dateFormat`, `overrides`, and `triggers → actions → requests`, where each request defines a `UrlTemplate`, `Method`, optional `RequestTemplate`, and optional `ResponseTemplate`. An empty or blank configuration SHALL parse into an empty configuration rather than an error, and a malformed configuration SHALL raise a descriptive parse error.

#### Scenario: Parse a valid configuration
- **WHEN** a YAML configuration defining a `BookPayment` trigger with `Find` and `Create` actions bound to named requests is parsed
- **THEN** the resulting `FinanceConfiguration` exposes the trigger, its actions, and the referenced request definitions

#### Scenario: Malformed configuration surfaces an error
- **WHEN** a malformed YAML configuration is parsed
- **THEN** the system raises an error indicating the configuration failed to parse

### Requirement: Request resolution by trigger and action
The system SHALL resolve a `RequestConfiguration` for a given trigger name and action name using case-insensitive matching, returning nothing when the trigger, action, or referenced request is absent.

#### Scenario: Resolve a configured request
- **WHEN** the provider resolves the request for trigger `BookPayment` and action `Create`
- **THEN** the system returns the `RequestConfiguration` named by that action

#### Scenario: Missing action resolves to nothing
- **WHEN** the provider resolves a request for an action that the trigger does not define
- **THEN** the system returns no request configuration

### Requirement: Template rendering of requests
The system SHALL render the `UrlTemplate` and, for non-GET methods, the `RequestTemplate` using a template engine applied to a template data model derived from the domain entity and connection base URL. The rendered URL and body SHALL substitute all referenced fields.

#### Scenario: Render URL and body from templates
- **WHEN** a request with `UrlTemplate` `{{{baseUrl}}}/payments/{{{paymentId}}}` and a JSON `RequestTemplate` is executed for a payment
- **THEN** the outgoing request targets the substituted URL
- **AND** the outgoing body contains the substituted field values

### Requirement: Authentication scheme selection and application
The system SHALL construct typed authentication options from the tenant's connection details, defaulting the scheme to OAuth when unspecified and reading keys case-insensitively. It SHALL select an authentication provider by scheme from {OAuth, Basic, NoAuth} and apply it to each outgoing request: OAuth as a Bearer token, Basic as a base64 `user:password` header, and NoAuth applying nothing.

#### Scenario: Apply OAuth bearer token
- **WHEN** a request is sent for a tenant whose scheme is OAuth
- **THEN** the request carries an `Authorization: Bearer <token>` header

#### Scenario: Apply Basic authentication
- **WHEN** a request is sent for a tenant whose scheme is Basic
- **THEN** the request carries an `Authorization: Basic <base64>` header derived from the configured username and password

#### Scenario: Missing required OAuth options rejected
- **WHEN** authentication options are created for OAuth without a token endpoint, client id, client secret, or scope
- **THEN** the system rejects the options as invalid

### Requirement: Per-tenant OAuth token caching
The system SHALL cache the OAuth access token per tenant and reuse it while it remains valid, refreshing only when the token is expired, near expiry (within a small validity margin), or incompatible with the current provider. A newly obtained token SHALL be persisted for reuse across subsequent requests.

#### Scenario: Reuse a still-valid cached token
- **WHEN** a request is made for a tenant whose cached token is still within its validity window
- **THEN** the system reuses the cached token without contacting the token endpoint

#### Scenario: Refresh an expired token
- **WHEN** a request is made for a tenant whose cached token has expired
- **THEN** the system obtains a new token from the token endpoint and persists it for reuse

### Requirement: Response normalisation
The system SHALL transform a successful provider response into a strongly typed internal result using the request's `ResponseTemplate` when present, and SHALL return an empty result when the provider returns no content or a not-found status.

#### Scenario: Normalise a provider response
- **WHEN** a provider returns a JSON payload for a configured request that defines a `ResponseTemplate`
- **THEN** the system maps the payload into the internal result according to the template

#### Scenario: Not-found returns empty result
- **WHEN** a provider returns a not-found status for a search request
- **THEN** the system returns an empty result rather than raising an error

### Requirement: Resilient execution and secret redaction
The system SHALL send integration requests through an HTTP client configured with a bounded timeout and a resilience pipeline (transient retry and circuit breaker), and SHALL surface non-transient provider errors as a communication error carrying the status code. Sensitive fields (secrets, passwords, tokens) MUST be redacted from any logged request or response detail.

#### Scenario: Transient failure is retried
- **WHEN** a provider responds with a transient server error
- **THEN** the client retries according to the configured resilience policy before failing

#### Scenario: Provider error is surfaced with status
- **WHEN** a provider responds with a non-transient client or server error
- **THEN** the system raises a communication error that includes the returned status code

#### Scenario: Secrets are redacted in logs
- **WHEN** a request or its error is logged
- **THEN** credential, password, and token values do not appear in the log output
