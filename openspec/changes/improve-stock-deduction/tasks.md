## 1. Data model & migrations

- [ ] 1.1 Add `ReservationItem` entity (`Id`, `ReservationId` FK, `VariantId`, `Quantity`, `TenantId`) and EF configuration; map on `InventoryDbContext`
- [ ] 1.2 Add `UNIQUE(tenant_id, order_id)` index on `reservations` and `ix_reservations_sweeper (status, expires_at)`
- [ ] 1.3 Add `UNIQUE(reservation_id, variant_id)` on `reservation_items`
- [ ] 1.4 Add outbox pending index `ix_outbox_pending (processed_on_utc) WHERE processed_on_utc IS NULL`
- [ ] 1.5 Generate and review the EF Core migration (additive, backward-compatible)

## 2. Shared contracts

- [ ] 2.1 Add required `ReservationId` to `StockReserved` (BREAKING, additive-required)
- [ ] 2.2 Add `ConfirmReservationCommand` (`OrderId`, `ReservationId`, tenant/user fields)
- [ ] 2.3 Confirm `ReleaseReservationCommand` shape and register message routing

## 3. CAS deduction core (capability: stock-deduction)

- [ ] 3.1 Add CAS deduct method to `IInventoryRepository` returning rows-affected (`UPDATE … WHERE variant_id=@v AND tenant_id=@t AND stock_available >= @qty`)
- [ ] 3.2 Add increment/add-back method to `IInventoryRepository` for release (`stock_available += qty`)
- [ ] 3.3 Rewrite `ReserveStocksCommandHandler`: sort items by `VariantId` asc; remove the optimistic-retry loop
- [ ] 3.4 Wrap deduction in one transaction: inbox insert → per-item CAS → `Reservation(Pending, now+15m)` → `ReservationItem` rows → outbox `StockReserved`
- [ ] 3.5 On any CAS `rows=0`: roll back, compensate Redis, publish `StockReservationFailed` (all-or-nothing)
- [ ] 3.6 Add bounded deadlock-retry around the transaction (catch deadlock, retry within configured bound)

## 4. Idempotency (capability: stock-deduction)

- [ ] 4.1 Make `MakeReservationConsumer` idempotent via inbox (`IdempotentConsumer<MakeReservation>` keyed by `OrderId`)
- [ ] 4.2 Handle `UNIQUE(tenant_id, order_id)` violation as already-processed (ACK & skip, never NACK)
- [ ] 4.3 Verify dedupe guard and deduction commit in the same transaction

## 5. Redis fast-gate (capability: stock-deduction)

- [ ] 5.1 Run `IRedisStockGateway.TryReserveAsync` (all-or-none) before opening the transaction
- [ ] 5.2 On cache miss, warm Redis from PostgreSQL (`SeedStockAsync`) then retry the gate
- [ ] 5.3 On CAS `rows=0` after gate pass, call `ReleaseAsync` to compensate the gate

## 6. Event reliability (capability: stock-event-reliability)

- [ ] 6.1 Enable transactional outbox (prefer MassTransit EF Core Outbox) for `StockReserved`/`StockReservationFailed`
- [ ] 6.2 Implement/enable polling publisher relay (`processed_on_utc IS NULL … FOR UPDATE SKIP LOCKED`), mark processed after publish
- [ ] 6.3 Thread `ReservationId` from `StockReserved` through the saga into `PersistOrderCommand`

## 7. Reservation lifecycle — release & TTL (capability: stock-reservation-lifecycle)

- [ ] 7.1 Implement `ReleaseReservationConsumer`: status-guarded, per-`ReservationItem` add-back, mark `Released`, Redis `ReleaseAsync`
- [ ] 7.2 Wire `OrderSaga` compensation to send `ReleaseReservationCommand` on stock-fail / reject / cancel / payment-fail
- [ ] 7.3 Implement TTL sweeper job (Hangfire/Quartz, ~1 min) expiring `Pending` past `ExpiresAt` with add-back + `Expired`
- [ ] 7.4 Ensure release-vs-expiry race applies add-back exactly once (status guard)

## 8. Reservation lifecycle — payment confirm (capability: stock-reservation-lifecycle)

- [ ] 8.1 Implement confirm handler/consumer for `ConfirmReservationCommand`: `Pending → Confirmed`, no stock change
- [ ] 8.2 Wire the payment-success signal to send `ConfirmReservationCommand` (or document as follow-up if payment integration is out of scope)

## 9. Consistency at scale (capability: stock-event-reliability)

- [ ] 9.1 Implement Redis ↔ PostgreSQL reconciliation job (reseed available counters from PostgreSQL)

## 10. Tests & verification

- [ ] 10.1 Unit: CAS returns rows=1 on sufficient, rows=0 on insufficient
- [ ] 10.2 Concurrency: N concurrent orders on one SKU → `sold == initial_stock`, zero oversell, no negative stock
- [ ] 10.3 Idempotency: redeliver `MakeReservation` twice → single deduction; concurrent duplicates → one commits
- [ ] 10.4 Deadlock: `[A,B]` vs `[B,A]` concurrent orders → no deadlock failure
- [ ] 10.5 Release: cancel restores per-variant stock; double release is a no-op
- [ ] 10.6 TTL: unpaid reservation past 15 min → stock restored, status `Expired`; cancel-vs-expiry add-back once
- [ ] 10.7 Confirm: paid reservation → `Confirmed`, stock unchanged, sweeper ignores it
- [ ] 10.8 Outbox: crash after commit before publish → event still delivered on recovery
- [ ] 10.9 Reconciliation: induced drift reseeds Redis; CAS still prevents oversell under drift
- [ ] 10.10 Load test: 1 hot SKU, 5k concurrent orders → zero oversell, bounded p99 latency
