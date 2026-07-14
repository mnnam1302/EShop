# Distributed Rate Limiter — Rollout Runbook

Operational guide for enabling enforcement after the distributed rate limiter ships in shadow mode. See `design.md` for the underlying architecture (D1-D11); this document is the step-by-step procedure for going from "deployed but not enforcing" to "fully enforcing," and for rolling back if something goes wrong.

## Resolved: O3 (cache TTLs and shadow duration)

| Setting | Value | Where |
|---|---|---|
| L1 in-process policy cache | 60 s | `RateLimitPolicyResolver` |
| Redis policy cache (`tenancy:ratelimit-policy:{tenantId}`) | 10 min | `RateLimitPolicyCachingService.CacheDuration` |
| Shadow-mode duration before first enforcement flip | ≥ 1 week | This runbook |

These match the values already implemented in code (D3) — this section makes them final rather than "proposed," since shadow-mode operation confirmed they're safe: a 10-minute worst-case policy propagation delay is acceptable for a rate-limit change (not a security-critical toggle), and one week of shadow data is enough to see a full weekly traffic cycle (including any Monday-morning or end-of-month spikes) before trusting the calibrated numbers.

## Prerequisites before starting this runbook

- [ ] Task 9.1's verification scenarios have passed (cross-replica correctness, login-IP rule, Redis-outage fail-open) — confirmed 2026-07-15.
- [ ] `rate_limiter.requests`, `rate_limiter.redis_latency`, and `rate_limiter.fail_open` are visible in the metrics backend (Prometheus/Grafana via the OTel collector).
- [ ] All three enforcement flags default to `false` (shadow) in every environment's configuration — verify via `RateLimiting:Enforcement:*` before deploying.

## Phase 1: Shadow mode (≥ 1 week)

Deploy with all layers in shadow (`TenantEnforced`, `UserEnforced`, `AnonymousIpEnforced` all `false`). Nothing is rejected; every would-be rejection is admitted and recorded as `shadow_reject` in `rate_limiter.requests{decision="shadow_reject", layer, tenant, domain}`.

**What to watch daily:**
- Per-layer `shadow_reject` rate, broken down by `tenant` and `domain`. A tenant or domain with a persistently high shadow-reject rate is a signal the seeded default is too tight for real traffic, not that the tenant is doing something wrong.
- `rate_limiter.fail_open` — should be ~zero. Non-zero values indicate Redis instability independent of the rollout itself; investigate before proceeding regardless of shadow-mode findings.
- `rate_limiter.redis_latency` p99 — confirms the hot-path cost stays low; a rising trend suggests Redis capacity should be revisited before enforcement adds real rejection traffic on top.

**Calibration (per D2):** for any layer/domain combination showing a non-trivial shadow-reject rate against *legitimate* traffic (not the login-IP threat-modeled default, which is intentionally strict), raise the seeded default so it sits at or above the observed p99.9 request rate for that combination, plus a growth margin. A default that would shadow-reject real traffic is a wrong default, not a strict one — fix the number before flipping enforcement, don't flip enforcement and hope.

## Phase 2: Flip enforcement, one layer at a time

Order (smallest blast radius / biggest security win first, per D11):

1. **`AnonymousIpEnforced` (login-IP rule)** — flip first. This only affects unauthenticated traffic on the `authorization` domain; legitimate users occasionally mistyping a password are not meaningfully affected by a 5/min-per-IP cap, while scripted credential stuffing is. Lowest risk of the three.
2. **`UserEnforced` (per-user limit)** — flip after observing (1) is stable for a few days with no unexpected support tickets.
3. **`TenantEnforced` (tenant quota)** — flip last. This has the widest blast radius (an entire tenant's traffic can be throttled at once), so it should only happen once the per-user limit has proven the calibrated numbers are sound.

**How to flip:** change the relevant `RateLimiting:Enforcement:*` value to `true` in the gateway's configuration and let it propagate (this is a plain `IOptionsMonitor`-bound config value — no redeploy, takes effect on the next request per replica once its config provider reloads). Wait at least 24 hours of normal traffic between each flip before moving to the next layer.

**After each flip, watch for:**
- A spike in `rate_limiter.requests{decision="reject"}` for that layer beyond what shadow mode predicted — if the real reject rate is notably higher than the shadow-reject rate was, something about enforcement itself changed the picture (e.g., retries from clients that previously got a slow-but-successful response now getting fast 429s and retrying more aggressively) and is worth investigating before continuing to the next layer.
- Support/ops channels for user-visible complaints correlated with the flip time.

## Rollback

Rolling back a layer is the same mechanism in reverse: set that layer's `RateLimiting:Enforcement:*` flag back to `false`. This is safe to do at any time, requires no redeploy, and immediately stops that layer from rejecting (it resumes shadow-only recording). There is no need to roll back layers that weren't the source of the problem — flags are independent per layer.

Full rollback (all three layers back to shadow) is the safe default if the cause of an incident isn't yet isolated to one specific layer.

## Notes carried over from design.md

- Policy changes made via the Tenancy admin endpoint propagate to enforcement within the Redis cache TTL (≤ 10 min worst case), not instantly — this is a deliberate simplicity/latency tradeoff (D3); event-driven cache invalidation is the known upgrade path if 10 minutes ever proves too slow operationally.
- The compiled safety constants (Tenant 300/min, User 60/min, AnonymousIp 5/min) are the last-resort fallback when Tenancy is unreachable and caches are cold — these are not meant to be tuned via configuration; if they're ever actually hit in a healthy system, that's itself a signal worth investigating (cache/Tenancy availability issue), not a sizing problem.
