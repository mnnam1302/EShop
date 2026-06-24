## Why

The current stock-reservation path (`ReserveStocksCommandHandler`) deducts stock with a read-modify-write plus an optimistic-lock retry loop, which collapses into retry storms and false rejections under hot-SKU contention, and it has **no idempotency guard** (a redelivered `MakeReservation` double-deducts), **no durable event publish** (a crash after commit loses `StockReserved` and strands the saga), and **no release path** (cancelled / unpaid / abandoned orders permanently leak stock). For a deduct-on-order platform with a payment step, these gaps cause oversell, lost inventory, and stuck orders. The full solution is already designed and signed off in `Inventory/docs/stock-deduction-solution-specification.md`; this change implements it.

## What Changes

- Replace the optimistic-retry deduction with an **atomic conditional UPDATE (CAS)** `WHERE stock_available >= qty` as the binding no-oversell mechanism, with deterministic multi-item lock ordering (sort by `VariantId`) and bounded deadlock-retry.
- Add **idempotency** independent of CAS: inbox dedupe plus a `UNIQUE(tenant_id, order_id)` guard on the reservation, committed in the **same transaction** as the deduction.
- Keep the **Redis Lua fast-gate** in front of the database (all-or-nothing), with compensation when the database rejects after the gate passed.
- Persist a **per-order `Reservation` (Pending, 15-min TTL) + per-variant `ReservationItem` rows** in the deduction transaction (enables release add-back and audit).
- Add the **release path**: `ReleaseReservationCommand` + `ReleaseReservationConsumer` (idempotent per-variant add-back) wired to saga compensation for stock-fail / reject / cancel / payment-fail.
- Add a **TTL sweeper** background job that expires `Pending` reservations older than 15 minutes and returns stock.
- Add the **payment-confirm path**: a new `ConfirmReservationCommand` that flips the hold to `Confirmed` on payment success (no stock change).
- Add a **transactional outbox** (polling publisher) for `StockReserved` / `StockReservationFailed` so events survive a post-commit crash, plus a **Redis ↔ Postgres reconciliation** job to heal cache drift.
- **BREAKING (integration contract):** `StockReserved` gains a required `ReservationId` field so the saga can thread it to `PersistOrderCommand` and the release path.

## Capabilities

### New Capabilities

- `stock-deduction`: The deduct-on-order write path executed as one transaction — Redis fast-gate, atomic CAS deduction (`stock_available >= qty`) as the no-oversell guard, deterministic multi-item lock ordering with bounded deadlock-retry, inbox + `UNIQUE(order)` idempotency (distinct from CAS), and creation of the `Reservation` hold + `ReservationItem` rows.
- `stock-reservation-lifecycle`: The reservation state machine (`Pending → Confirmed | Released | Expired`) and its transitions — `ReleaseReservationCommand` per-variant add-back, the 15-minute TTL sweeper for abandoned orders, and the payment-gated `ConfirmReservationCommand`.
- `stock-event-reliability`: Durable and consistent integration — transactional outbox (polling publisher) for `StockReserved` (now carrying `ReservationId`) / `StockReservationFailed`, and Redis fast-gate consistency with Postgres-authoritative reconciliation.

### Modified Capabilities

<!-- No existing openspec/specs/ capability covers Inventory stock or the Order saga; all work above is net-new capability. The sibling change `place-order-saga-design` owns the saga's structural design; this change consumes its contracts and only requires the additive `StockReserved.ReservationId` field. -->

## Impact

- **Inventory service**: `ReserveStocksCommandHandler` (rewrite to gate → CAS → hold → items → outbox), `IInventoryRepository` (CAS deduct + increment-release methods), `Reservation` aggregate (+ new `ReservationItem` entity), `MakeReservationConsumer`, new `ReleaseReservationConsumer` / confirm handling, EF migrations (`reservations` unique index, `reservation_items` table, outbox pending index), new TTL-sweeper and reconciliation background jobs.
- **Order service**: `OrderSaga` compensation wiring (`ReleaseReservationCommand` on failure/cancel/payment-fail), threading `ReservationId` from `StockReserved` into `PersistOrderCommand`, payment-confirm signal (may be follow-up if payment integration is out of branch scope).
- **Shared contracts** (`EShop.Shared.Contracts`): `StockReserved` += `ReservationId` (**BREAKING**, additive-required — coordinate Inventory+Order deploy); new `ConfirmReservationCommand`.
- **Infrastructure**: Redis (gate + reconciliation), RabbitMQ/MassTransit (outbox relay, new consumers), Hangfire/Quartz (TTL sweeper, reconciliation), PostgreSQL schema migrations.
- **Authoritative design**: `Inventory/docs/stock-deduction-solution-specification.md` (SSOT) and `stock-deduction-deduct-on-order-design.md` (rationale).
