## 1. Data model & migrations

- [x] 1.1 Add `ReservationItem` entity (`Id`, `ReservationId` FK, `VariantId`, `Quantity`, `TenantId`) and EF configuration; map on `InventoryDbContext`
- [x] 1.2 Add `UNIQUE(tenant_id, order_id)` index on `reservations` and `ix_reservations_sweeper (status, expires_at)`
- [x] 1.3 Add `UNIQUE(reservation_id, variant_id)` on `reservation_items`
- [x] 1.4 Add outbox pending index `ix_outbox_pending (processed_on_utc) WHERE processed_on_utc IS NULL`
- [x] 1.5 Generate and review the EF Core migration (additive, backward-compatible)

## 2. Shared contracts

- [x] 2.1 Add required `ReservationId` to `StockReserved` (BREAKING, additive-required)
- [x] 2.2 Add `ConfirmReservationCommand` (`OrderId`, `ReservationId`, tenant/user fields)
- [x] 2.3 Confirm `ReleaseReservationCommand` shape and register message routing

## 3. CAS deduction core (capability: stock-deduction)

- [x] 3.1 Add CAS deduct method to `IInventoryRepository` returning rows-affected (`UPDATE … WHERE variant_id=@v AND tenant_id=@t AND stock_available >= @qty`)
- [x] 3.2 Add increment/add-back method to `IInventoryRepository` for release (`stock_available += qty`)
- [x] 3.3 Rewrite `ReserveStocksCommandHandler`: sort items by `VariantId` asc; remove the optimistic-retry loop
- [x] 3.4 Wrap deduction in one transaction: inbox insert → per-item CAS → `Reservation(Pending, now+15m)` → `ReservationItem` rows → outbox `StockReserved`
- [x] 3.5 On any CAS `rows=0`: roll back, compensate Redis, publish `StockReservationFailed` (all-or-nothing)
- [x] 3.6 Add bounded deadlock-retry around the transaction (catch deadlock, retry within configured bound)

## 4. Idempotency (capability: stock-deduction)

- [x] 4.1 Make `MakeReservationConsumer` idempotent via inbox (`IdempotentConsumer<MakeReservation>` keyed by `OrderId`)
- [x] 4.2 Handle `UNIQUE(tenant_id, order_id)` violation as already-processed (ACK & skip, never NACK)
- [x] 4.3 Verify dedupe guard and deduction commit in the same transaction

## 5. Redis fast-gate (capability: stock-deduction)

- [x] 5.1 Run `IRedisStockGateway.TryReserveAsync` (all-or-none) before opening the transaction
- [x] 5.2 On cache miss, warm Redis from PostgreSQL (`SeedStockAsync`) then retry the gate
- [x] 5.3 On CAS `rows=0` after gate pass, call `ReleaseAsync` to compensate the gate

## 6. Event reliability (capability: stock-event-reliability)

- [x] 6.1 Enable transactional outbox (prefer MassTransit EF Core Outbox) for `StockReserved`/`StockReservationFailed`
- [x] 6.2 Implement/enable polling publisher relay (`processed_on_utc IS NULL … FOR UPDATE SKIP LOCKED`), mark processed after publish
- [x] 6.3 Thread `ReservationId` from `StockReserved` through the saga into `PersistOrderCommand`

## 7. Reservation lifecycle — release & TTL (capability: stock-reservation-lifecycle)

- [x] 7.1 Implement `ReleaseReservationConsumer`: status-guarded, per-`ReservationItem` add-back, mark `Released`, Redis `ReleaseAsync`
- [x] 7.2 Wire `OrderSaga` compensation to send `ReleaseReservationCommand` on stock-fail / reject / cancel / payment-fail
- [x] 7.3 Implement TTL sweeper job (Hangfire/Quartz, ~1 min) expiring `Pending` past `ExpiresAt` with add-back + `Expired`
- [x] 7.4 Ensure release-vs-expiry race applies add-back exactly once (status guard)

## 8. Reservation lifecycle — payment confirm (capability: stock-reservation-lifecycle)

- [x] 8.1 Implement confirm handler/consumer for `ConfirmReservationCommand`: `Pending → Confirmed`, no stock change
- [x] 8.2 Wire the payment-success signal to send `ConfirmReservationCommand` (or document as follow-up if payment integration is out of scope)

## 9. Consistency at scale (capability: stock-event-reliability)

- [x] 9.1 Implement Redis ↔ PostgreSQL reconciliation job (reseed available counters from PostgreSQL)

## 10. Tests & verification

- [x] 10.1 Unit: CAS returns rows=1 on sufficient, rows=0 on insufficient
- [ ] 10.2 Concurrency: N concurrent orders on one SKU → `sold == initial_stock`, zero oversell, no negative stock (integration test — requires real PostgreSQL)
- [x] 10.3 Idempotency: redeliver `MakeReservation` twice → single deduction; UNIQUE(tenant_id, order_id) guard verified via handler path (unit); concurrent test requires real DB
- [x] 10.4 Deadlock: `[A,B]` vs `[B,A]` → deterministic sort order proven in unit test; retry logic bounded
- [x] 10.5 Release: cancel restores per-variant stock; double release is a no-op (status guard verified in unit test)
- [x] 10.6 TTL: unpaid reservation past 15 min → status `Expired`; cancel-vs-expiry add-back once (status guard verified)
- [x] 10.7 Confirm: paid reservation → `Confirmed`, stock unchanged (unit verified)
- [x] 10.8 Outbox: `StockReserved` enqueued before commit (sequencing verified in unit test)
- [ ] 10.9 Reconciliation: induced drift reseeds Redis (integration test — requires Redis + PostgreSQL)
- [ ] 10.10 Load test: 1 hot SKU, 5k concurrent orders → zero oversell (load test — requires deployed environment)
