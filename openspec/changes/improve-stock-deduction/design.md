## Context

The Inventory `ReserveStocksCommandHandler` currently deducts stock with a read-modify-write plus an optimistic-lock retry loop, has no idempotency guard, publishes `StockReserved` directly after `SaveChanges` (lost on a post-commit crash), and has no release path. The platform is **deduct-on-order with a payment step** (order provisional until paid). The full solution is signed off in two authoritative docs which this design references rather than restates:

- **SSOT** — `Inventory/docs/stock-deduction-solution-specification.md` (final decisions D1–D10 + O3, data model DDL, contract changes, correctness transaction, Definition-of-Ready).
- **Rationale** — `Inventory/docs/stock-deduction-deduct-on-order-design.md` (deep-dives §14 CAS≠idempotency, §15 outbox polling vs CDC, §16 deadlock, §17 cross-doc consistency).

Constraints: .NET 8 microservices, EF Core + PostgreSQL, MassTransit/RabbitMQ, Redis, multi-tenant (`IScoped`). The Order saga is the event-sourced `OrderSaga : AggregateSaga`. The sibling change `place-order-saga-design` owns the saga structure; this change consumes its contracts.

## Goals / Non-Goals

**Goals:**
- No oversell under hot-SKU concurrency, enforced by the database.
- Exactly-once-effect deduction under at-least-once delivery, **separate from** the oversell guard.
- Durable `StockReserved`/`StockReservationFailed` so the saga never strands.
- Mandatory release path (cancel / payment-fail / TTL) so deduct-on-order never leaks stock.
- Deadlock-free multi-item reservations.
- Redis as a rebuildable fast gate; PostgreSQL always authoritative.

**Non-Goals:**
- Replacing the saga's structural design (owned by `place-order-saga-design`).
- Full payment-provider integration — only the `ConfirmReservationCommand` seam; the confirm trigger may be a follow-up. TTL release keeps correctness if so.
- Inventory bucketing / queue peak-shaving / movement ledger (future scale levers).
- Rewriting the Order README's legacy saga diagrams (separate cleanup).

## Decisions

### D2 — Atomic conditional UPDATE (CAS) over optimistic-retry
Deduct via `UPDATE … SET stock_available = stock_available - @qty WHERE variant_id=@v AND tenant_id=@t AND stock_available >= @qty`; `rows=1` success, `rows=0` insufficient. The predicate *is* the no-oversell invariant, applied atomically — no read-modify-write window, no retry loop.
- **Alternatives:** optimistic lock + retry (collapses into retry storms and false rejections on hot SKUs); `SELECT FOR UPDATE` (holds locks across round-trips, lower throughput). Rejected — see rationale §4.

### D7 — Idempotency is a separate concern from CAS
CAS is stateless about the caller, so it cannot dedupe a redelivered message. Guard with **inbox dedupe + `UNIQUE(tenant_id, order_id)` on `Reservation`**, committed in the **same transaction** as the deduction. A read-first check is only an optimization, never the guard.
- **Alternatives:** rely on CAS alone (double-deducts on redelivery — the bug this fixes); read-first only (races under concurrent duplicates). Rejected — see rationale §14.

### D8 — Deterministic lock ordering + bounded deadlock-retry
Sort items by `VariantId` ascending before issuing per-item CAS updates so all transactions acquire row locks in the same total order (breaks circular wait); add a bounded retry for any residual deadlock.
- **Alternatives:** single batched `IN (...)` (lock order not guaranteed across statements); `SELECT FOR UPDATE ORDER BY` (extra round-trip, reintroduces read-then-write); per-SKU serialization (overkill). Rejected — see rationale §16.

### D5/D9/D10 — Two-stage hold with payment-gated confirm and 15-minute TTL
Create a per-order `Reservation(Pending, ExpiresAt = now + 15m)` plus per-variant `ReservationItem` rows in the deduction transaction. `ConfirmReservationCommand` moves `Pending → Confirmed` on payment (no stock change). Release (`ReleaseReservationCommand`) and the TTL sweeper move `Pending → Released/Expired` and add stock back per `ReservationItem`. All transitions out of `Pending` are idempotent (status-guarded).

### O3 — Per-order Reservation + per-variant ReservationItem rows
Store line items so the release/expiry add-back is self-contained (no cross-service read into Order) and auditable.
- **Alternative:** re-derive quantities from the persisted order — couples Inventory to Order data and can fire before the order is persisted. Rejected.

### D6 — Transactional outbox, polling publisher
Write events to an outbox in the deduction transaction; relay with a polling publisher (`WHERE processed_on_utc IS NULL … FOR UPDATE SKIP LOCKED`). Prefer MassTransit's EF Core Outbox over hand-rolling.
- **Alternatives:** direct publish after `SaveChanges` (current — lost on crash); CDC/Debezium (near-real-time but needs replication slots + Kafka + ops weight — overkill now). Deferred — see rationale §15.

### D3/D4 — Redis fast-gate, PostgreSQL authoritative
Redis Lua all-or-nothing gate runs before the transaction to shed stampede; on CAS `rows=0` after the gate passed, compensate Redis. A reconciliation job reseeds Redis from PostgreSQL to heal drift.

### Contract change — `StockReserved += ReservationId`
Additive-required field so the saga threads the id to `PersistOrderCommand` and the release path. New `ConfirmReservationCommand`.

## Risks / Trade-offs

- **`StockReserved` gains a required field (BREAKING)** → Deploy the shared contract, then Inventory (producer) and Order (consumer) together; the field is additive so a brief mixed-version window only delays consumption, it does not corrupt state.
- **Redis ↔ Postgres dual-write drift** → PostgreSQL CAS is authoritative (never oversells); reconciliation job reseeds Redis; worst case is a transient lost sale.
- **Payment-confirm trigger may not exist this branch** → TTL sweeper releases unconfirmed holds after 15m, preserving correctness; confirm wiring tracked as follow-up.
- **At-least-once outbox relay** → consumers remain idempotent (inbox + unique key), so duplicate broker delivery is harmless.
- **Deduction transaction now spans gate-compensation, CAS, hold, items, outbox** → keep the transaction short and row-scoped; gate runs outside the transaction; ordering keeps locks deadlock-free.
- **Background jobs (TTL sweeper, reconciliation) add moving parts** → idempotent, status-guarded operations safe to run repeatedly and on multiple instances.

## Migration Plan

1. Schema migration: `UNIQUE(tenant_id, order_id)` on `reservations`, new `reservation_items` table, outbox pending index. Backward-compatible (additive).
2. Ship shared contract (`StockReserved.ReservationId`, `ConfirmReservationCommand`), then Inventory + Order together.
3. Phased rollout matching the spec/implementation phases: deduction core → reliability (outbox + ReservationId) → release + payment → reconciliation + load test.
4. **Rollback:** new tables/columns and background jobs are additive; disabling the new handler path reverts to prior behavior without data loss. The `UNIQUE(order)` guard is the only constraint that could reject writes — validated by the idempotency tests before enabling.

## Open Questions

- Confirm trigger source for `ConfirmReservationCommand` (payment service event vs saga step) — depends on whether payment integration lands in this branch.
- Whether to retain the entity `RowVersion` for admin/manual stock edits (not needed on the order path; CAS supersedes it).
- O3-adjacent: whether a materialised `Committed`/`ReservedStock` counter is needed for reporting, or derive from holds (defaulting to derive).
