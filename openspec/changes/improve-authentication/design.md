## Context

The EShop platform is a .NET multi-tenant SaaS system using per-tenant RSA key pairs for JWT signing, Redis-backed token caching for session management, and a shared `MultiTenantJwtBearerHandler` across all microservices. The authentication pipeline serves three distinct traffic types through a single code path: external API requests (frontend/Postman), service-to-service HTTP calls, and MassTransit consumer contexts.

The proposal identifies six capabilities addressing critical security bugs (token collision, header spoofing), reliability gaps (Redis SPOF, unhandled exceptions), and architectural friction (S2S overhead, fragile consumer patterns, missing key rotation).

Current state summary:
- **Cache key**: `authorization:tokens:{userId}` — shared between external sessions and S2S tokens
- **S2S auth**: Full JWT generation + 4 Redis round-trips per internal call via `SystemInternalJwtTokenFactory`
- **Consumer auth**: Manual `SetSystemUserContext` / `ClearSystemUserContext` with try/finally
- **Error handling**: Mixed `Result<T>` and thrown exceptions across the pipeline
- **Gateway**: YARP reverse proxy at `EShop.ApiGateway` with `UseAuthentication()` but no header sanitization
- **Key rotation**: `TenantKeyProvider.RotateKeyPairAsync` — `throw new NotImplementedException()`

## Goals / Non-Goals

**Goals:**

- Fix token cache collision so S2S calls never invalidate active user sessions
- Prevent header spoofing by stripping internal-only headers at the API gateway
- Make the auth handler exception-safe with consistent `AuthenticateResult.Fail` responses
- Reduce S2S auth cost by eliminating unnecessary JWT generation and cache writes for internal traffic
- Provide a composable scope pattern for consumer/background job auth context
- Add resilience so Redis unavailability degrades gracefully rather than causing total outage
- Implement RSA key rotation with overlap period for active token validation
- Maintain backward compatibility — no breaking changes to service-level auth attributes or API contracts

**Non-Goals:**

- **Service mesh / mTLS (Layer 0 — Network Security)**: Mutual TLS and zero-trust networking (Istio/Linkerd) are infrastructure-level concerns managed by DevOps/platform teams. This improvement operates at the application layer and works regardless of whether mTLS is deployed. mTLS and application-level auth are complementary, not alternatives — mTLS answers "is this service allowed to talk to me?" while our auth answers "on behalf of which user/tenant?"
- **OAuth2 token exchange for S2S (RFC 8693)**: Formal token delegation (exchanging a user token for an audience-scoped S2S token via an auth server) adds a critical-path dependency and significant complexity. Header-based propagation with gateway stripping (D2+D4) provides equivalent security at current service count. Token exchange becomes relevant at 50+ services with complex trust relationships.
- **Multi-factor authentication / step-up auth**: MFA is an end-user feature to be built on top of this infrastructure (e.g., adding `auth_level` claim + `[RequireMfaAuth]` attribute), not part of the infrastructure hardening itself.
- **Centralized session management migration**: The hybrid JWT + cache approach is retained and improved (D6 resilience), not replaced with pure server-side sessions. The hybrid gives cryptographic integrity (JWT signature) plus instant revocation (cache check).
- **Auth-level rate limiting / DDoS protection**: Operational tuning of existing gateway rate limiters (`app.UseRateLimiter()`) and WAF rules. The login endpoint already has account lockout protection (`User.MaxFailedAccessAttemptsBeforeLockout`). Not an application architecture change.
- **External identity provider migration** (Auth0, Keycloak) — this is an internal hardening effort, not a platform migration
- **Changing the JWT token format or claim structure** — existing tokens remain valid
- **Adding OAuth2 flows** (authorization code, PKCE) — current username/password flow is unchanged
- **Per-service authorization policy changes** — `[RequirePermission]`, `[RequireFeature]`, `[RequireSystemUser]` remain as-is
- **Changing MassTransit transport or message contracts** — the scoped ConsumeFilter (D5) uses existing `IIntegrationEvent` fields, no message contract changes needed

## Decisions

### D1: Token Cache Isolation — Scoped Cache Keys

**Decision**: Introduce a `tokenScope` dimension to cache keys. External user sessions use `authorization:tokens:user:{userId}`, S2S tokens use `authorization:tokens:s2s:{userId}:{correlationId}` with short TTL.

**Rationale**: The root cause is that `SystemInternalJwtTokenFactory.GenerateToken()` writes to the same cache key as `LoginQueryHandler`. Adding a scope prefix cleanly separates the two without changing the cache infrastructure.

**Alternatives considered**:
- *Separate cache instance for S2S*: Over-engineered, adds operational complexity
- *Don't cache S2S tokens at all*: Would require the auth handler to know which tokens are S2S vs external — leaks concern into the handler
- *Use request-scoped correlation ID only*: Hard to look up during validation without passing extra context

**Impact on validation**: `MultiTenantJwtBearerHandler` will check both `user:{userId}` and `s2s:{userId}:*` scopes when validating. S2S tokens get a short TTL (5 minutes) and are auto-evicted.

### D2: Header Security — Gateway-Level Stripping

**Decision**: Add YARP request transform in `EShop.ApiGateway` that removes all `eshop-*` custom headers from incoming external requests before forwarding to backend services.

**Rationale**: This is the simplest, most reliable fix. The gateway is already the single entry point for external traffic. Stripping headers at the edge means backend services don't need to distinguish internal vs external requests — if headers are present, they're trusted.

**Alternatives considered**:
- *Validate headers against JWT claims in HttpRequestUserDataProvider*: Adds complexity to every service, still requires knowing if a request is external
- *HMAC-sign internal headers*: Adds shared secret management, more complex than needed
- *Dedicated internal-only ports/endpoints*: Infrastructure change, harder to maintain

**Implementation**: Use YARP `RequestHeaderRemove` transforms for headers: `eshop-user-type`, `eshop-user-id`, `eshop-tenant-id`, `eshop-action-user-id`.

### D3: Auth Handler Hardening — Exception-Safe Pipeline

**Decision**: Wrap JWT validation in `MultiTenantJwtBearerHandler.HandleAuthenticateAsync()` with try-catch, converting all exceptions to `AuthenticateResult.Fail`. Unify error handling to use `Result<T>` throughout the handler's internal pipeline.

**Rationale**: Currently `tokenManager.GetPrincipalFromTokenAsync()` throws `SecurityTokenException`, `SecurityTokenExpiredException`, `InvalidOperationException` which are unhandled in the handler. These should produce 401 responses, not 500s.

**Alternatives considered**:
- *Change JwtTokenManager to return Result<T>*: Better long-term but larger change surface. We'll do the handler-level catch first, then refactor `JwtTokenManager` to `Result<T>` as a separate task.

### D4: Service-to-Service — Short-Lived Internal JWT (No Cache Validation)

**Decision**: For S2S calls, `SystemInternalJwtTokenFactory` generates a short-lived JWT with `audience = "internal"` and 30-second expiry. No cache write, no refresh token generation. `MultiTenantJwtBearerHandler` checks the audience claim — if `"internal"`, skip cache validation and rely on RSA signature + short TTL only.

**Rationale**: This eliminates the 4 Redis round-trips and cache collision while maintaining cryptographic proof of identity (Zero Trust compliant). Each S2S token is signed with the tenant's RSA key, carrying full user context (`userId`, `tenantId`, `userType`) in JWT claims. The 30-second TTL makes replay attacks impractical — the token expires before an attacker could meaningfully exploit it.

Performance comparison:
- Current: 1 RSA sign + 4 Redis RTTs + 1 RSA verify + 1 Redis RTT = 5 Redis RTTs + 2 RSA ops
- Revised: 1 RSA sign + 1 RSA verify = 2 RSA ops, **0 Redis RTTs**

**Alternatives considered**:
- *Header-only auth (no JWT)*: Simplest but violates Zero Trust — no cryptographic proof of identity. Any compromised internal service could forge identity headers. Security depends entirely on D2 gateway stripping (single point of failure).
- *Lightweight HMAC-signed tokens*: Symmetric key shared across all services — compromise of one service leaks the secret for all. Custom protocol with no standard tooling.
- *API keys for S2S*: Designed for static identity ("I am Service A"), not dynamic identity propagation ("I am Service A acting on behalf of User X in Tenant Y"). Would still need JWT or headers for user context, making the API key redundant. Also requires building key management infrastructure (~500+ lines).
- *Keep current approach but fix cache keys only (D1 alone)*: Leaves the 4 Redis round-trips and RSA overhead per S2S call. Over-engineered — S2S tokens live <1 second, cache-based revocation adds no value for them.

**Implementation**:
1. `SystemInternalJwtTokenFactory.GenerateToken()` → add `audience = "internal"` claim, set expiry to 30 seconds, remove `userTokenCachingService.AddAsync()` call and refresh token generation
2. `MultiTenantJwtBearerHandler.ValidateTokenInCacheAsync()` → check audience claim first; if `"internal"`, return success immediately (skip cache lookup)
3. Continue using `Authorization: Bearer <jwt>` scheme — the handler already processes Bearer tokens, no scheme change needed
4. Custom headers (`eshop-user-type`, etc.) are still set by `AddUserContext()` for `HttpRequestUserDataProvider` compatibility, but identity is also verifiable from the JWT claims

**Important**: The existing external JWT path (long-lived, cache-validated) remains unchanged. Only the internal S2S path is modified. The `audience` claim is the sole discriminator — no new authentication schemes or handlers needed.

### D5: Consumer Auth Scope — MassTransit Scoped ConsumeFilter + IDisposable Fallback

**Decision**: Two-pronged approach:

**5a. MassTransit consumers**: Create a scoped `IFilter<ConsumeContext<TMessage>>` that automatically extracts `TenantId`, `ActionUserId`, and `ActionUserType` from `IIntegrationEvent` messages and sets the system user context before the consumer executes. Cleanup happens in the pipeline's `finally` block. Consumers no longer contain any auth code.

```
// Registered ONCE per service in MassTransit configuration:
cfg.UseConsumeFilter(typeof(SystemUserContextConsumeFilter<>), context,
    x => x.Include(type => type.HasInterface<IIntegrationEvent>()));

// Filter implementation (scoped, resolved per-message via DI):
public class SystemUserContextConsumeFilter<TMessage>
    : IFilter<ConsumeContext<TMessage>>
    where TMessage : class, IIntegrationEvent
{
    private readonly IUserDetailsProvider _userDetailsProvider;

    public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
    {
        _userDetailsProvider.SetSystemUserContext(
            context.Message.TenantId,
            context.Message.ActionUserId,
            context.Message.ActionUserType);
        try
        {
            await next.Send(context);  // consumer executes here
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}

// Consumer becomes pure business logic:
public class TenantCreatedConsumer : IConsumer<ITenantCreated>
{
    public async Task Consume(ConsumeContext<ITenantCreated> context)
    {
        // Auth context already set by pipeline — just do business logic
        await _mediator.SendAsync(command, context.CancellationToken);
    }
}
```

**5b. Non-MassTransit contexts** (DbInitializer, Hangfire jobs, `FeatureService` internal calls): Introduce `SystemUserScope : IDisposable` that wraps `SetSystemUserContext` and calls `ClearSystemUserContext` on dispose. Provide a factory method on `IUserDetailsProvider`.

```
// For background jobs, DB initializers, and service-internal scope changes:
using var scope = userDetailsProvider.CreateSystemUserScope(tenantId);
// ... business logic
// auto-cleared on dispose
```

**Rationale**: `IIntegrationEvent` already carries `TenantId`, `ActionUserId`, and `ActionUserType` — all the data needed to set auth context. MassTransit's scoped filter infrastructure (see [Scoped Middleware Filters](https://masstransit.io/documentation/configuration/middleware/scoped)) resolves a new filter instance from DI per message, matching the scoped lifetime of `IUserDetailsProvider`. This is strictly superior to the manual try/finally pattern:
- **Zero developer effort** — consumers contain no auth code
- **Pipeline-enforced cleanup** — impossible to forget `ClearSystemUserContext`
- **Centralized configuration** — registered once per service, applied to all `IIntegrationEvent` consumers
- **Consistent data source** — tenant context always comes from the message, not from developer-provided parameters

The `IDisposable` pattern covers non-consumer contexts where MassTransit pipeline is not available.

**Alternatives considered**:
- *IDisposable only (original D5)*: Works but still requires every consumer to manually create a scope — doesn't leverage MassTransit's pipeline which is purpose-built for this
- *AsyncLocal-based ambient context*: Thread-safe but harder to reason about with async/await task continuations
- *Custom MassTransit middleware (non-scoped)*: Would not have access to scoped `IUserDetailsProvider` from DI — scoped filters solve this by resolving from the message's DI scope

### D6: Auth Pipeline Resilience — Graceful Degradation

**Decision**: When Redis is unavailable:
1. `UserTokenRedisCachingService.GetAsync` returns `null` instead of throwing `BadRequestException`
2. `MultiTenantJwtBearerHandler` skips cache validation when cache returns null and relies on JWT signature validation alone (degraded mode)
3. Log a warning when operating in degraded mode
4. `TenantKeyProvider` uses in-memory fallback for RSA keys already loaded in the current process

**Rationale**: JWT signature validation alone is still secure — tokens are cryptographically signed. Cache validation adds revocation capability (defense-in-depth). Losing revocation temporarily is better than total platform outage.

**Alternatives considered**:
- *Redis Sentinel/Cluster for HA*: Infrastructure concern, should be done regardless but doesn't eliminate the need for code-level resilience
- *Fail closed (current behavior)*: Too aggressive — a 30-second Redis blip shouldn't lock out every user on every tenant
- *Circuit breaker pattern*: Good addition on top of graceful degradation, but the handler still needs to know what to do when the circuit is open

**Trade-off**: During Redis outage, token revocation (logout) won't take effect immediately. Acceptable for short outages; monitoring should alert on degraded mode.

### D7: RSA Key Rotation — Dual-Key Overlap

**Decision**: Implement `RotateKeyPairAsync` with a dual-key strategy:
1. Generate new key pair, store as "active" for signing
2. Keep previous key pair as "previous" for validation (overlap period = key expiry)
3. `JwtTokenManager.GenerateAccessToken` always signs with "active" key
4. `JwtTokenManager.GetPrincipalFromTokenAsync` validates against "active" first, falls back to "previous" if `KeyId` matches

**Rationale**: Tokens signed with the old key are still valid until they expire. Abrupt key replacement would invalidate all active sessions.

**Cache key structure**:
- `authorization:keys:{tenantId}:active` — current signing key
- `authorization:keys:{tenantId}:previous` — previous key (auto-expires)

**Alternatives considered**:
- *JWKS endpoint*: Standard approach but requires services to fetch keys over HTTP — adds latency and another SPOF. Better suited for a future phase if external IdP integration is planned.
- *Key versioning array*: Store array of all historical keys — unbounded growth, harder to reason about

## Risks / Trade-offs

**[Risk] S2S internal JWT has a 30-second replay window** → Mitigation: The short TTL makes replay impractical. Traffic between services is on a private network/VPC. The token is scoped to a specific tenant's RSA key, so compromising one tenant doesn't affect others. If mTLS is added later (infrastructure layer), the replay window drops to effectively zero.

**[Risk] Degraded mode during Redis outage loses token revocation** → Mitigation: JWT tokens already have short TTL (configurable via `JwtOptions.AccessTokenExpiryMinutes`). Monitoring alerts on degraded mode. Revocation resumes when Redis recovers.

**[Risk] Dual-key rotation increases cache memory per tenant** → Mitigation: Only 2 keys per tenant at most. RSA key pairs are small (~2KB PEM). For 1000 tenants = ~4MB total — negligible.

**[Risk] Consumer scope pattern requires updating all existing consumers** → Mitigation: For MassTransit consumers, the scoped `ConsumeFilter` is applied at the pipeline level — consumers just need their manual `SetSystemUserContext`/`ClearSystemUserContext` code removed. For non-consumer contexts (DbInitializer, Hangfire, FeatureService), search for `SetSystemUserContext` → replace with `using var scope = ...`. Old pattern still works during transition (backward compatible).

**[Risk] Cache key migration (D1) during rolling deployment** → Mitigation: The new `MultiTenantJwtBearerHandler` will check both old-format and new-format cache keys during a transition period. After full deployment, remove old-format key check.

## Migration Plan

**Phase 1 — Critical Fixes (no behavioral change for services)**:
1. Deploy gateway header stripping (D2) — zero risk, additive only
2. Deploy auth handler hardening (D3) — bug fix, no behavior change for valid tokens
3. Deploy cache key isolation (D1) — backward compatible with dual-key-lookup transition

**Phase 2 — Architecture Improvements**:
4. Deploy S2S short-lived internal JWT (D4) — changes `SystemInternalJwtTokenFactory` to 30s audience-scoped JWT without cache write
5. Deploy consumer scope pattern (D5) — backward compatible, migrate consumers incrementally
6. Deploy auth pipeline resilience (D6) — changes failure behavior, needs monitoring setup first

**Phase 3 — Maturity**:
7. Deploy RSA key rotation (D7) — new capability, no existing behavior change

**Rollback**: Each phase deploys independently. Shared library changes are backward compatible within each phase. If issues arise, revert the shared library NuGet version.

## Open Questions

1. **S2S call authentication in test environment**: With D4, S2S calls still use JWT Bearer (with `audience = "internal"` and 30s TTL). BDD tests via `ApiTestContextBase` already generate JWTs — the only change is adding the `audience` claim and skipping cache writes. Existing test infrastructure should work with minimal modification.

2. **Redis degraded mode duration threshold**: How long should we operate in degraded mode before escalating? Should there be a circuit breaker that eventually fails closed after prolonged Redis unavailability?

3. **Key rotation trigger**: Should key rotation be automated on a schedule (e.g., every 90 days via Hangfire job), or manual-only via admin API endpoint?
