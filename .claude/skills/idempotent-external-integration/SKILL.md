---
name: idempotent-external-integration
description: >
  Solution design for preventing duplicate bookings when integrating with
  external accounting providers (ERP, payment gateway, banking API) in
  distributed finance systems. Use when designing job-based external API
  integrations, handling dual-write problems, or building retry-safe
  booking pipelines in multi-node deployments.
---

# Idempotent External Integration Pattern

Prevents duplicate bookings when a finance service communicates with any
third-party accounting provider over an unreliable network.

## The problem

Two systems (Finance DB + external provider) must stay in sync without a
shared transaction. A window of inconsistency always exists between
"provider accepted" and "Finance recorded." Crashes, timeouts, or network
failures in that window cause either duplicate bookings or lost bookings.

This is the **dual-write problem** — it cannot be eliminated, only managed.

## Core principle

You cannot guarantee exactly-once delivery to an external system. You can:

1. **Prevent** duplicates (make them extremely unlikely)
2. **Detect** duplicates (know when prevention fails)
3. **Correct** duplicates (reverse/void the extra entry)

## Solution: 3 layers, each with one job

```
Layer 1 — Distributed lock          → Optimization (serialize concurrent access)
Layer 2 — Verify-before-act         → Prevention  (catch prior successful calls)
Layer 3 — Reconciliation            → Detection   (independent cross-system audit)
```

No layer alone is sufficient. Each covers a specific failure mode.

### What each layer does and does NOT do

**Lock (Redis SETNX + expiration + ownership release via Lua)**
- Does: prevents two nodes from calling the provider simultaneously
- Does not: prevent duplicates from crash-gap scenarios
- Remove the lock → system is still correct, just less efficient
- Lock TTL must exceed API timeout to avoid expiration mid-call

**Verify-before-act (GET before POST)**
- Does: detect a prior successful booking before retrying
- Does not: work if GET queries the wrong scope or provider has eventual consistency
- Rule: if verify fails (network error on GET) → abort, do NOT proceed to POST

**Reconciliation (scheduled cross-system comparison)**
- Does: detect any discrepancy regardless of how it happened
- Does not: prevent duplicates — it finds them after the fact
- Required in finance — answers "if all code fails, how fast do we know?"

## Generic interface

```csharp
public interface IExternalBookingGateway
{
    /// Check if a booking with this correlation ID already exists.
    /// Returns provider's booking reference if found, null otherwise.
    Task<ExternalBookingRef?> FindByCorrelationAsync(string correlationId);

    /// Create a booking. Pass correlation ID for provider to store.
    /// Returns provider's booking reference on success.
    Task<ExternalBookingRef> CreateBookingAsync(
        string correlationId, BookingPayload payload);
}

public record ExternalBookingRef(string BookingId, string? DocumentNo);
```

Each provider adapter implements this interface differently:

| Provider capability         | FindByCorrelation queries by        | Reliability   |
|-----------------------------|--------------------------------------|---------------|
| Stores client correlation ID | That field (deterministic)          | High          |
| Has idempotency key          | Idempotency key (deterministic)     | High          |
| No client ID support         | Business fields — heuristic         | Needs recon   |

## Complete job flow

```
 1  Pick transaction WHERE Status = PENDING AND NextRetryAt <= NOW

 2  Acquire distributed lock (TTL = 2 × API timeout)
    → Fail to acquire → skip, another node is handling it

 3  try:
        existing = FindByCorrelationAsync(transaction.Id)

 4      if existing != null:
            transaction.BookingId = existing.BookingId
            transaction.Status   = BOOKED          // ← self-healing
            save → release lock → done

 5      try:
            result = CreateBookingAsync(transaction.Id, payload)
            transaction.BookingId = result.BookingId
            transaction.Status   = BOOKED
            save → release lock → done

 6      catch BusinessException:
            transaction.Status = FAILED_BUSINESS    // terminal, no retry
            save → release lock → done

 7      catch NetworkException / TimeoutException:
            if transaction.AttemptCount >= MAX_ATTEMPTS:
                transaction.Status = FAILED_EXHAUSTED   // terminal + alert ops
            else:
                transaction.AttemptCount++
                transaction.NextRetryAt = now + backoff(AttemptCount)
                transaction.Status = PENDING
            save → release lock → done

 8  catch Exception (from step 3 — FindByCorrelation failed):
        // Cannot verify → MUST NOT proceed to POST
        transaction.AttemptCount++
        transaction.NextRetryAt = now + backoff(AttemptCount)
        save → release lock → done

 9  finally:
        distributedLock.Dispose()                   // release in ALL cases
```

### Why each step matters

- **Step 4 (self-healing):** when GET finds an existing entry, code MUST mark
  BOOKED and save the provider's reference. Skipping the POST without updating
  status leaves the transaction stuck in PENDING forever.
- **Step 8 (abort on verify failure):** if you cannot confirm whether a prior
  booking exists, posting again risks a duplicate. Abort and retry later.
- **Step 9 (finally):** lock release must happen regardless of outcome. Use
  the disposable pattern (`using` in C#) to guarantee this.

## Transaction states

```
PENDING ──→ BOOKED              (success or self-healed)
        ──→ FAILED_BUSINESS     (provider rejected — terminal)
        ──→ FAILED_EXHAUSTED    (max retries reached — alert ops)
        ──→ PENDING             (transient failure — retry with backoff)
```

## Reconciliation job (Layer 3)

```
Scheduled daily (or hourly for providers without correlation ID support):

1. Query Finance DB:  all transactions WHERE Status = BOOKED
2. Query provider:    all entries matching correlation IDs
3. Compare:
   - Finance BOOKED + provider has entry     → OK
   - Finance BOOKED + provider missing       → Alert: phantom booking
   - Provider has entry + Finance not BOOKED → Auto-fix: mark BOOKED
   - Both exist but amounts differ           → Alert: data mismatch
4. Output: reconciliation report + alerts
```

## Key rules

1. Lock is optimization, not correctness. Removing it must not break the system.
2. Lock TTL > API timeout. Prevents lock expiration during in-flight calls.
3. Never POST without verifying first. If verify fails, abort.
4. Self-heal on verify hit. Always update status + save provider reference.
5. Exponential backoff on transient failures. Do not hammer a recovering provider.
6. Reconciliation is mandatory in finance. Not a backup plan — a control.
7. Correlation ID should flow end-to-end. Finance ID → provider field → GET query.