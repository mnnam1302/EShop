# distributed-rate-limiting

## ADDED Requirements

### Requirement: Platform-wide limit enforcement
The API gateway SHALL enforce rate limits using counters shared across all gateway replicas, such that the effective limit is independent of the number of replicas and of which replica serves a request.

#### Scenario: Limit holds across replicas
- **WHEN** a user with a limit of 10 requests per minute sends 11 requests distributed across two gateway replicas within one minute
- **THEN** exactly 10 requests are admitted and the 11th is rejected

### Requirement: Atomic counter evaluation
Rate-limit checks SHALL evaluate and update counters atomically, so that concurrent requests can never admit more than the configured limit, and all replicas SHALL derive time from a single clock source.

#### Scenario: Concurrent burst at the boundary
- **WHEN** a client with 5 remaining permits sends 20 concurrent requests
- **THEN** exactly 5 requests are admitted and 15 are rejected

### Requirement: Tenant-qualified partitioning
Per-user counters SHALL be partitioned by the combination of tenant identifier and user identifier, never by username alone, and per-tenant counters SHALL be partitioned by tenant identifier.

#### Scenario: Same username in different tenants
- **WHEN** users named `admin` in tenant A and tenant B each send requests
- **THEN** each consumes only their own tenant-qualified counter and neither affects the other's remaining limit

### Requirement: Layered limits in one evaluation
For authenticated requests the gateway SHALL evaluate the tenant quota and the per-user limit together in a single atomic operation, admitting the request only when both allow it, and reporting which scope caused a rejection.

#### Scenario: Tenant quota exhausted, user under limit
- **WHEN** a user with remaining personal permits sends a request while the tenant's quota is exhausted
- **THEN** the request is rejected and the response identifies the tenant quota as the exceeded limit

### Requirement: Policy resolution with caching and fallback
The gateway SHALL resolve applicable rules from an in-process cache backed by Redis and, on miss, the Tenancy internal endpoint; the per-request hot path MUST NOT call the Tenancy service directly. Resolution SHALL fall back most-specific-first: tenant `(domain, scope)` → tenant `(*, scope)` → system tenant `(domain, scope)` → system tenant `(*, scope)` → compiled safety defaults. Policy changes SHALL take effect within the configured cache TTL. Tenants without a policy SHALL be negatively cached.

#### Scenario: Tenant override wins over platform default
- **WHEN** a tenant has a rule for `(*, User)` and the system tenant also defines `(*, User)`
- **THEN** the tenant's rule governs that tenant's users

#### Scenario: Policy change propagates within TTL
- **WHEN** a tenant's policy is updated in Tenancy
- **THEN** the gateway enforces the new values no later than the configured cache TTL after the update

#### Scenario: Tenancy service unavailable
- **WHEN** the Tenancy service is unreachable and a tenant's policy is not cached
- **THEN** the gateway enforces compiled safety defaults and continues serving requests

### Requirement: Anonymous login protection
The gateway SHALL enforce a per-client-IP sliding-window limit on the authorization (login) route for unauthenticated requests, partitioned per IP.

#### Scenario: Excess login attempts from one IP
- **WHEN** a client IP exceeds the configured per-minute login attempt limit
- **THEN** further login attempts from that IP within the window are rejected with 429

#### Scenario: Other IPs unaffected
- **WHEN** one IP is being rejected for excess login attempts
- **THEN** login attempts from a different IP are admitted normally

### Requirement: Client response contract
Rejected requests SHALL receive HTTP 429 with a `Retry-After` header and a JSON:API error body with code `rate_limit_exceeded` whose detail distinguishes the exceeded scope (user limit vs tenant quota). Admitted requests on rate-limited routes SHALL carry `RateLimit-Limit`, `RateLimit-Remaining`, and `RateLimit-Reset` headers. Rejected requests MUST NOT be queued at distributed layers.

#### Scenario: Rejection response shape
- **WHEN** a request is rejected by the user-scope limit
- **THEN** the response is 429 with a `Retry-After` header and a JSON:API error identifying the user limit as exceeded

#### Scenario: Success carries limit headers
- **WHEN** a request is admitted on a rate-limited route
- **THEN** the response includes `RateLimit-Limit`, `RateLimit-Remaining`, and `RateLimit-Reset`

### Requirement: Fail-open resilience
When the distributed counter store is unavailable or exceeds the configured evaluation timeout, the gateway SHALL fail open: requests are admitted subject to per-node in-memory fallback limits, a circuit breaker prevents per-request timeout costs, and a fail-open metric is emitted. A rate-limiter fault MUST NOT cause request failures.

#### Scenario: Redis outage
- **WHEN** Redis is unreachable and a request arrives
- **THEN** the request is admitted (subject to in-memory fallback limits) and the fail-open metric is incremented

#### Scenario: Recovery
- **WHEN** Redis becomes reachable again and the circuit closes
- **THEN** distributed enforcement resumes without gateway restart

### Requirement: Bounded counter memory
Every distributed counter key SHALL carry an expiry no longer than the time to fully replenish its limit (or twice its window), so stored state is proportional to currently active clients.

#### Scenario: Idle client key expires
- **WHEN** a client stops sending requests
- **THEN** its counter keys expire from Redis within the replenishment period

### Requirement: Shadow mode and per-layer enforcement
Each enforcement layer (login IP, user limit, tenant quota) SHALL support a shadow mode in which would-be rejections are logged and counted but requests are admitted; switching a layer between shadow and enforce SHALL be a configuration change requiring no redeploy.

#### Scenario: Shadow layer admits and records
- **WHEN** a request exceeds a limit whose layer is in shadow mode
- **THEN** the request is admitted and a shadow-reject metric with tenant, layer, and domain tags is recorded

#### Scenario: Flipping to enforce
- **WHEN** a layer's flag is changed from shadow to enforce
- **THEN** subsequent excess requests on that layer are rejected with 429 without redeploying the gateway

### Requirement: Rate-limiter observability
The gateway SHALL emit metrics for every rate-limit decision (allow, reject, shadow-reject) tagged with layer, tenant, and domain, plus counter-store latency and fail-open occurrences.

#### Scenario: Decision metrics
- **WHEN** requests are evaluated by the rate limiter
- **THEN** per-decision counters tagged with layer, tenant, and domain are observable in telemetry
