# Distributed Rate Limiter — Design

## Context

EShop is a multi-tenant SaaS fronted by a YARP API gateway; all external traffic passes through it. The gateway already calls `UseRateLimiter` with policies from `EShop.Shared.JsonApi.RateLimiting`, but an audit found the current implementation defective:

- Counters live in `System.Threading.RateLimiting` per process — N gateway replicas multiply every limit by N, and which limit a client sees depends on load-balancer routing.
- The `UserBasedRateLimiting` policy is attached only to the tenancy route. The authorization route (login) is covered only by the `GlobalLimiter`, which returns `GetNoLimiter` for unauthenticated requests — login is effectively unlimited.
- All anonymous users share the single partition key `"anonymous-user"` (5 req/min total across the platform).
- The per-tenant `ConcurrencyLimiter` is per-node, so it does not cap a tenant platform-wide.
- Queue limits (3–5) hold gateway request threads for requests that will mostly still be rejected.

Constraints: Redis and StackExchange.Redis (with Polly resilience) are already in the stack via `EShop.Shared.Cache`; tenant identity is available at the gateway after `UseAuthentication`; anonymous traffic is rare on this platform (few anonymous-accessible APIs); services communicate internally via MassTransit/RabbitMQ, so internal traffic does not need request rate limiting.

## Goals / Non-Goals

**Goals:**

- Limits enforced platform-wide, correct under gateway scale-out (shared counters, atomic updates, single clock).
- Rate-limit rules are tenant configuration owned by the Tenancy service, changeable at runtime without deploy; platform defaults included.
- Hot-path cost ≤ 1 Redis round trip per request; rules resolved from in-process memory.
- Bounded memory: counter keyspace proportional to currently active clients, not clients ever seen.
- Clear client contract: 429 + `Retry-After` + IETF `RateLimit-*` headers + JSON:API error body.
- Limiter failure never becomes a platform outage: fail open, degraded, loudly.
- Shadow-mode rollout before enforcement.
- Core limiter reusable by Finance for outbound provider throttling later.

**Non-Goals:**

- Finance outbound throttling (follow-up change; core designed for reuse).
- Organization-level limits (tenant + user scopes cover current risks).
- Endpoint-granular rules (route-group `Domain` granularity; schema extensible).
- Multi-region/edge synchronization; distributed concurrency limiting; per-service policy enforcement (services keep only local self-protection).
- Request queueing/soft-throttling of rejected requests.

## Decisions

### D1. Rules live in Tenancy: `TenantSetting.RateLimitPolicy` owned JSON type

`Tenant` already owns a one-per-tenant `TenantSetting` child. It gains an owned `RateLimitPolicy` mapped to a JSON column (`OwnsOne(...).ToJson()` with `OwnsMany(p => p.Rules)`), holding `RateLimitRule { Domain, Scope, Unit, RequestsPerUnit, Burst? }` with `RateLimitScope { Tenant, User, AnonymousIp }` and `RateLimitUnit { Second, Minute, Hour, Day }`.

- Why not appsettings: no per-tenant granularity, deploy per change, support cannot act at runtime.
- Why not `TenantFeatures`: features are plan entitlements (on/off state); a rate-limit policy is a structured configuration document — different shape, owner, cadence (single responsibility).
- Why not EF complex types (`ComplexProperty`): EF Core 8 complex types do not support collections; owned-entity `ToJson()` does.
- Why JSON column, not a rules table: the policy is read/written as a whole document, never queried per-rule; rules per tenant are few; schema evolves without migration churn.
- Rule schema is the Lyft descriptor model adapted: `Domain` = gateway route group (`tenancy`, `authorization`, `catalog`, `*`), `Scope` = partition key type, plus `Burst` for token-bucket capacity.

Write side: `Tenant.SetRateLimitPolicy(policy)` behavior method guarded by a validation `Specification` (`RequestsPerUnit > 0`; `Burst >= RequestsPerUnit` when set; unique `(Domain, Scope)`; bounded rule count). Exposed via a Tenancy admin endpoint restricted to system/support users — tenants cannot raise their own quotas.

### D2. Platform defaults on the system tenant; tenants store overrides only

Rules that belong to no customer tenant — anonymous-IP login rule, defaults for tenants without a policy — live on the **system tenant's** `TenantSetting.RateLimitPolicy`. Resolution is most-specific-wins with fallback:

```
tenant (domain, scope) → tenant (*, scope) → system (domain, scope) → system (*, scope) → compiled safety constants
```

`AddDefaultTenantSetting()` does **not** seed rate-limit rules: absence means inherit, so platform defaults can change later without migrating every tenant row. The compiled constants are the honest last resort for "Tenancy unreachable and caches cold" — code, not configuration.

**Sizing the default numbers.** The limits hardcoded in the current implementation are arbitrary and are explicitly not carried forward. Every seeded default must have a written rationale, derived in two passes:

1. Initial values from capacity budgeting, top-down: measured sustainable throughput of the constraining downstream (DB/service p99 headroom) → reserve margin → divide across expected active tenants (tenant quota) → divide by expected active users per tenant (user bucket). Burst = short-window client behavior (page load, SPA fan-out), not sustained rate. Security rules (login) are sized from threat math, not capacity — 5/min/IP class.
2. Calibration from shadow mode, bottom-up: compare seeded values against observed legitimate traffic percentiles (e.g., user limit ≥ p99.9 of per-user request rates; tenant quota ≥ max observed tenant rate + growth margin) and adjust before enforcement. A default that would shadow-reject legitimate traffic is a wrong default, not a strict one.

### D3. Policy distribution: dedicated cache path, TTL propagation

```
Hot path:      L1 MemoryCache (~60s)
On L1 miss:    Redis key tenancy:ratelimit:{tenantId} (TTL ~10 min)
On Redis miss: GET internal Tenancy endpoint /api/tenants/{id}/settings/rate-limit-policy
               (service discovery + Polly, ~2s timeout) → populate Redis + L1
```

- New `RateLimitPolicyCacheKeyProvider` + `RateLimitPolicyCachingService` in `EShop.Shared.Cache`, separate from `TenantFeaturesRedisCachingService`.
- The internal Tenancy read endpoint runs with system scope (`TenantSetting` is `IScoped`).
- Negative caching: "no policy → inherit defaults" is cached explicitly, so default-configured tenants don't trigger HTTP fallback every L1 expiry.
- Single-flight guard (per-key `SemaphoreSlim`) prevents fallback stampede.
- Propagation is TTL-based (worst case ≈ Redis TTL). Alternative — `TenantRateLimitPolicyUpdated` integration event with gateway eviction — deferred until TTL propagation proves too slow.

### D4. Algorithms: token bucket (Tenant/User), sliding window counter (AnonymousIp)

- Token bucket for tenant quota and per-user limits: burst-friendly for interactive clients, 2 fields per key, matches current in-memory semantics.
- Sliding window counter for the login IP rule: smooths window-boundary bursts, 2 counters per key.
- Rejected alternatives: fixed window (boundary burst = 2× limit), sliding window log (memory grows per request, not per client).
- Every counter key gets a TTL (time-to-refill-full, or 2× window) set in the same script — keyspace self-cleans to the active client set (~100 bytes/key; ~10k active clients ≈ single-digit MB).

### D5. Atomicity via Lua; Redis is the single clock

Read-compute-write executes as one atomic `EVALSHA` Lua script (SHA cached at startup). One script checks **tenant quota + user bucket together** in a single round trip — no partial state, half the network cost. The script calls Redis `TIME` internally, so all gateway replicas share one clock (no skew-minted tokens). Return contract: `(allowed, remaining, retry_after_ms)` per checked scope.

- Rejected: locks (serialize gateways on Redis), sorted-set log (memory), app-server timestamps (clock skew).
- The Lua scripts are the highest-risk artifact: unit-tested against real Redis (Testcontainers), reviewed standalone.

### D6. Key design: tenant hash-tagged, always tenant-qualified

```
rl:{<tenantId>}:quota:<domain>        tenant token bucket
rl:{<tenantId>}:u:<userId>:<domain>   user token bucket
rl:{_}:ip:<ip>:<domain>               anonymous sliding window
```

Hash tag `{tenantId}` co-locates a tenant's keys in one cluster slot, keeping the two-key Lua script legal under Redis Cluster. User keys are explicitly `{tenantId}:{userId}` — never bare username — eliminating any cross-tenant collision regardless of token username format.

### D7. .NET integration: custom `RateLimiter` over Redis, standard middleware

A new `EShop.Shared.RateLimiting` library implements a custom `RateLimiter` (`AcquireAsyncCore` → EVALSHA) registered through the standard `AddRateLimiter`/`AddPolicy`, so `UseRateLimiter` and YARP per-route `RateLimiterPolicy` keep working. Policy attached to **all** routes.

- Rejected: `RedisRateLimiting` NuGet (third-party on the security perimeter; less control over fail-open, keys, metrics); Envoy-style sidecar limiter service (extra hop and deployable, unjustified at this scale).
- The library exposes the raw limiter core independently of ASP.NET so Finance can reuse it for outbound throttling.

### D8. Layering; drop the per-tenant concurrency cap

```
Layer 0  in-memory per-node guard (protects the node itself; no Redis)
Layer 1  distributed tenant quota (fairness / noisy neighbor)
Layer 2  distributed user bucket; login-route IP rule for anonymous
```

The existing per-tenant `ConcurrencyLimiter` is removed: per-node it protects nothing platform-wide, and the distributed tenant rate quota subsumes its intent; distributed concurrency leasing (acquire + release-on-response) is disproportionate complexity. `QueueLimit = 0` at distributed layers — decide instantly, let `Retry-After` drive client backoff. Enforcement policy: evaluated once at the gateway; services keep only local in-memory self-protection; MassTransit traffic is governed by consumer concurrency/prefetch, not request limiters.

### D9. Client contract

- Rejection: `429` + `Retry-After` (from the script's computed refill time) + JSON:API error body (`code: "rate_limit_exceeded"`, detail distinguishing user-limit vs tenant-quota).
- Success responses carry `RateLimit-Limit`, `RateLimit-Remaining`, `RateLimit-Reset` (IETF draft names; the `X-` prefix is deprecated per RFC 6648).
- Throttling is a normal response, never a thrown exception through `ExceptionHandlingMiddleware`.

### D10. Fault tolerance: fail open, degraded, loud

1. Hard ~50 ms timeout on the limiter's Redis call.
2. Polly circuit breaker (extend `RedisResiliencePolicyProvider`) — when Redis is down the gateway stops paying the timeout per request.
3. While open: fail open into Layer-0 in-memory limiters with configured limits applied per node (bounded over-admission, never reject-everyone, never fully unguarded).
4. `rate_limiter.fail_open` metric + alert; Redis health check already present.

Counter data needs no durability: Redis restart resets counters → brief over-admission, accepted.

### D11. Observability and rollout

- OTel counters via `EShop.Shared.Diagnostics`: `rate_limiter.requests{decision=allow|reject|shadow_reject, layer, tenant, domain}`, `rate_limiter.redis_latency`, `rate_limiter.fail_open`.
- Shadow mode first: evaluate + log/count would-be rejections without enforcing, per-layer enforcement flag. Enforce login IP rule first (smallest blast radius, biggest security win), then user buckets, tenant quota last.
- Per-tenant reject rate doubles as a product signal (plan upgrade candidates).

## Risks / Trade-offs

- [Redis on the request hot path adds latency] → single EVALSHA per request, ~50 ms cap, circuit breaker, Layer-0 pre-filter rejects floods without Redis.
- [Fail-open over-admits during Redis outage] → accepted by design; bounded by per-node in-memory degradation; alerted via `fail_open` metric.
- [Lua script bug throttles wrongly or not at all] → standalone review, Testcontainers integration tests, shadow mode surfaces misbehavior before enforcement.
- [Policy propagation delay (TTL)] → worst case ≈ Redis TTL (~10 min), documented; event-driven eviction is the known upgrade path.
- [Login IP rule needs the real client IP behind proxies/CDN] → resolve forwarded-headers configuration for the actual ingress topology before enforcement (open question O2).
- [Tenancy API unavailable on cold cache] → negative caching + single-flight + compiled safety constants; limiter never blocks on Tenancy in the hot path.
- [Enforcement surprises legitimate integrations] → shadow period with per-tenant shadow-reject analysis before each layer is flipped.
- [Shared `"anonymous-user"` fix changes behavior on tenancy route] → covered by shadow rollout; anonymous traffic is rare on this platform.

## Migration Plan

1. Tenancy first (additive): nullable JSON column migration for `TenantSetting`, domain model + specification, admin write endpoint, internal read endpoint, system-tenant default policy seeded (includes `authorization/AnonymousIp 5/min`).
2. Shared libraries: `EShop.Shared.RateLimiting` (Lua + custom `RateLimiter` + options), cache components, metrics.
3. Gateway: replace `ConfigureRateLimiters`, attach policy to all routes, deploy in **shadow mode** (evaluate, log, don't enforce; old behavior effectively preserved minus removed concurrency cap).
4. Observe shadow metrics ≥ 1 week; tune system defaults from real traffic.
5. Flip enforcement flags per layer: login IP → user buckets → tenant quota.
6. Rollback: any layer's flag back to shadow (config change, no deploy); full rollback = previous gateway image.

## Open Questions

- O1: `RateLimitPolicy` on the existing `TenantSetting` (recommended, one JSON column) vs a new sibling child entity (`TenantRateLimitSetting`) if display-settings vs throttle-policy separation inside Tenancy is preferred.
- O2: Ingress topology — is there a trusted proxy/CDN chain requiring `ForwardedHeaders` configuration for correct client IPs on the login rule?
- O3: Confirm cache TTLs (proposed: L1 60 s, Redis 10 min) and shadow-mode duration (proposed: 1 week).
