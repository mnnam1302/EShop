# Distributed Rate Limiter

## Why

The gateway's current rate limiter is both non-distributed and mis-wired: counters are per-process (so N gateway replicas multiply every limit by N), the `UserBasedRateLimiting` policy is attached only to the tenancy route (login on the authorization route is effectively unlimited — open to brute-force), all anonymous users share a single `"anonymous-user"` partition, and the per-tenant concurrency cap only applies per node. As a multi-tenant SaaS, EShop needs limits that are enforced platform-wide, configurable per tenant as a product attribute, and safe under gateway scale-out.

## What Changes

- Replace the in-memory `System.Threading.RateLimiting` policy in `EShop.Shared.JsonApi.RateLimiting` with distributed rate limiting backed by Redis, using atomic Lua scripts (token bucket for tenant/user scopes, sliding window counter for IP scope), with Redis `TIME` as the single clock.
- Introduce a rate-limit policy model owned by the Tenancy service: `TenantSetting` gains an owned `RateLimitPolicy` (JSON column) holding `RateLimitRule` entries (`Domain`, `Scope`, `Unit`, `RequestsPerUnit`, `Burst`). Platform defaults — including the anonymous-IP login rule — live on the system tenant's policy; customer tenants store only overrides.
- Add a Tenancy admin endpoint (system/support-restricted) to set a tenant's rate-limit policy, and an internal read endpoint for consumers.
- Add a dedicated policy distribution path: `RateLimitPolicyCachingService` + key provider in `EShop.Shared.Cache` (separate from `TenantFeatures` — single responsibility), read as gateway L1 memory cache → Redis → Tenancy API fallback, with negative caching and TTL-based propagation.
- Enforce at the gateway on all YARP routes: distributed tenant quota + per-user token bucket for authenticated traffic; per-IP rule for the login route (anonymous traffic is otherwise rare on this platform). Partition keys are always tenant-qualified (`{tenantId}:{userId}`).
- Rejected requests receive `429` with `Retry-After` and a JSON:API error body distinguishing user-limit vs tenant-quota; successful responses carry IETF-style `RateLimit-Limit` / `RateLimit-Remaining` / `RateLimit-Reset` headers. No request queueing at distributed layers.
- Fault tolerance: hard timeout on the Redis check, Polly circuit breaker, fail-open with degradation to per-node in-memory limits, `fail_open` metric and alerting. Counter loss on Redis restart is accepted (brief over-admission).
- Observability and rollout: OpenTelemetry counters tagged by decision/layer/tenant; shadow mode (log would-be rejections without enforcing) before enforcement, flipped per layer via configuration flag.
- Deferred to a follow-up change: outbound throttling of Finance calls to tenants' accounting providers (will reuse the same Redis/Lua core).

Behavior change (not API-breaking): routes that previously had no effective limit — notably login — will begin returning `429` once enforcement is switched on after the shadow period.

## Capabilities

### New Capabilities

- `rate-limit-policy-management`: Tenancy owns rate-limit rules as tenant configuration — domain model (`TenantSetting.RateLimitPolicy` owned JSON type), validation invariants, system-tenant platform defaults, admin write endpoint, internal read endpoint.
- `distributed-rate-limiting`: Platform-wide request limiting at the API gateway — Redis/Lua limiter core in a shared library, policy resolution with layered fallback and caching, enforcement on all routes, the 429/headers client contract, fail-open resilience, observability, and shadow-mode rollout.

### Modified Capabilities

None — no existing spec covers rate limiting; current limiter behavior is implementation-only and is replaced wholesale.

## Impact

- **Tenancy**: `Tenant`/`TenantSetting` domain model + EF configuration + migration; new admin and internal endpoints; validation specification.
- **ApiGateway (ReverseProxy)**: rate limiter registration replaced; policy attached to all routes; IP extraction for the login rule.
- **Shared**: new `EShop.Shared.RateLimiting` library (Lua scripts, custom `RateLimiter` over StackExchange.Redis, options); `EShop.Shared.JsonApi.RateLimiting` replaced; `EShop.Shared.Cache` gains `RateLimitPolicyCachingService`/key provider; `EShop.Shared.Diagnostics` gains limiter metrics.
- **Dependencies/infrastructure**: Redis moves onto the request hot path for counters (one `EVALSHA` per request, ~50 ms cap, circuit-broken); no new external dependencies.
- **Out of scope**: Finance outbound provider throttling (follow-up change); organization-level limits; multi-region synchronization; endpoint-granular rules (schema extensible via `Domain` if needed later).
