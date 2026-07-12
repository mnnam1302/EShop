# Tasks — distributed-rate-limiter

## 1. Tenancy domain model (D1, D2)

- [x] 1.1 Add `RateLimitPolicy`, `RateLimitRule`, `RateLimitScope`, `RateLimitUnit` types to `EShop.Tenancy.Domain` and the optional `RateLimitPolicy` property on `TenantSetting` (resolve O1: extend `TenantSetting` vs sibling entity before starting)
- [x] 1.2 Add `Tenant.SetRateLimitPolicy(...)` behavior guarded by a validation `Specification` (positive rate, `Burst >= RequestsPerUnit`, unique `(Domain, Scope)`, bounded rule count) with unit tests covering each invariant rejection
- [x] 1.3 Add EF configuration `OwnsOne(...).ToJson()` + `OwnsMany(Rules)` and the additive nullable-column migration (configuration code done; migration generation/apply is a user-owned step, see below)
- [x] 1.4 Seed the system tenant's default policy including the `authorization`/`AnonymousIp` per-minute login rule, with a written sizing rationale per rule (D2 sizing); verify new tenants get no seeded rules

## 2. Tenancy API endpoints

- [x] 2.1 Admin write endpoint (system/support-restricted) to set a tenant's policy via command handler; tests: system admin succeeds, tenant user forbidden, invalid policy rejected
- [x] 2.2 Internal service-to-service read endpoint returning a tenant's stored policy with system scope, distinguishing "no policy" from "tenant not found"; integration tests for both cases

## 3. Policy distribution (D3)

- [x] 3.1 Add `RateLimitPolicyCacheKeyProvider` and `RateLimitPolicyCachingService` to `EShop.Shared.Cache` (Redis TTL ~10 min, separate from TenantFeatures)
- [x] 3.2 Implement gateway-side policy resolver: L1 MemoryCache (~60 s) → Redis → Tenancy HTTP fallback (service discovery + Polly, ~2 s timeout), with negative caching and per-key single-flight guard
- [x] 3.3 Implement most-specific-first rule resolution (tenant → system tenant → compiled safety constants) with unit tests for each fallback level

## 4. Shared limiter core (D4, D5, D6)

- [ ] 4.1 Create `EShop.Shared.RateLimiting` project; define limiter abstractions and options usable without ASP.NET (reusable by Finance later)
- [ ] 4.2 Write the token-bucket Lua script (tenant + user keys checked atomically in one call, Redis `TIME` as clock, TTL on keys, returns `(allowed, remaining, retry_after_ms)` per scope)
- [ ] 4.3 Write the sliding-window-counter Lua script for IP scope with the same return contract and TTL behavior
- [ ] 4.4 Implement script loading/`EVALSHA` execution over StackExchange.Redis with tenant-hash-tagged key builders (`rl:{tenantId}:...`)
- [ ] 4.5 Testcontainers integration tests against real Redis: exact admission at the limit under concurrent bursts, refill over time, cross-"replica" correctness (two limiter instances, one Redis), tenant-qualified user keys isolated across tenants, key expiry after idle period

## 5. ASP.NET integration (D7, D8)

- [ ] 5.1 Implement the custom `RateLimiter` (`AcquireAsyncCore` → limiter core) and partition logic: tenant+user for authenticated requests, IP for anonymous on the authorization domain, `QueueLimit = 0`
- [ ] 5.2 Replace `ConfigureRateLimiters` in `EShop.Shared.JsonApi.RateLimiting`: register distributed policies, keep Layer-0 in-memory node guard, remove the per-tenant `ConcurrencyLimiter` and the shared `"anonymous-user"` partition
- [ ] 5.3 Attach `RateLimiterPolicy` to all YARP routes (including authorization) in gateway config; resolve O2 (forwarded-headers configuration for real client IP) for the login rule

## 6. Client contract (D9)

- [ ] 6.1 Emit `RateLimit-Limit` / `RateLimit-Remaining` / `RateLimit-Reset` headers on admitted responses from the script return values
- [ ] 6.2 Implement the rejection response: 429 + `Retry-After` + JSON:API error body (`rate_limit_exceeded`, detail distinguishing user limit vs tenant quota); integration tests for both rejection scopes and success-header presence

## 7. Resilience (D10)

- [ ] 7.1 Add hard ~50 ms timeout and circuit breaker for limiter Redis calls (extend `RedisResiliencePolicyProvider`)
- [ ] 7.2 Implement fail-open path: circuit open → per-node in-memory fallback limits, `rate_limiter.fail_open` metric; test Redis-outage admission and recovery without restart

## 8. Observability and shadow mode (D11)

- [ ] 8.1 Add OTel counters in `EShop.Shared.Diagnostics`: `rate_limiter.requests{decision, layer, tenant, domain}`, `rate_limiter.redis_latency`, `rate_limiter.fail_open`
- [ ] 8.2 Implement per-layer shadow/enforce flags (config-reloadable, no redeploy): shadow evaluates and counts `shadow_reject` but admits; tests for shadow admission and flag flip to enforce

## 9. Verification and rollout preparation

- [ ] 9.1 End-to-end run via Aspire: multi-replica gateway against one Redis exercising the cross-replica scenario, login IP rule, policy change propagation within TTL, and Redis-outage fail-open
- [ ] 9.2 Document the rollout runbook (shadow ≥ 1 week, calibration of defaults from shadow percentiles per D2 sizing, enforcement order: login IP → user → tenant quota, rollback = flag to shadow) and resolve O3 (TTLs, shadow duration)
