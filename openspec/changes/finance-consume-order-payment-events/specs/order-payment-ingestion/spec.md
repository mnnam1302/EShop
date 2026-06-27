## ADDED Requirements

### Requirement: Create finance account from order event

The Finance service SHALL consume the order "awaiting payment" integration event and create exactly one finance `Account` per `(TenantId, OrderId)`, capturing the order total, currency, buyer reference, and the requested `PaymentFrequency`.

#### Scenario: First delivery creates an account

- **WHEN** an order awaiting-payment event is consumed for an order that has no existing finance account
- **THEN** a new `Account` is created for that `(TenantId, OrderId)` with status `AwaitingSchedule`
- **AND** a payment schedule is generated for the account's total using the requested `PaymentFrequency`

#### Scenario: Default frequency when none supplied

- **WHEN** the order event carries no `PaymentFrequency`
- **THEN** the account is created with `PaymentFrequency = OneOff`

### Requirement: Idempotent event consumption

Finance consumers SHALL be idempotent under at-least-once delivery: processing the same integration event more than once SHALL NOT create duplicate accounts, duplicate instalments, or duplicate provider bookings.

#### Scenario: Redelivered order event is ignored

- **WHEN** an order awaiting-payment event is consumed a second time for an order that already has a finance account
- **THEN** no second account is created
- **AND** the message is acknowledged without error

#### Scenario: Redelivered payment event is ignored

- **WHEN** a payment-received event for an instalment that is already `Paid` is consumed again
- **THEN** the instalment remains `Paid` with no additional state change
- **AND** the message is acknowledged without error

### Requirement: Record payment receipts

Finance SHALL consume payment-received integration events and mark the referenced instalment `Paid`, recording the paid amount and timestamp.

#### Scenario: Payment marks instalment paid

- **WHEN** a payment-received event references a known `Pending` or `Booked` instalment
- **THEN** that instalment transitions to `Paid`
- **AND** the account's outstanding balance is reduced by the instalment amount

#### Scenario: Payment for unknown instalment is rejected

- **WHEN** a payment-received event references an instalment that does not exist on any account
- **THEN** the event is treated as a failure (not silently acknowledged) so it can be retried or dead-lettered

### Requirement: Emit payment lifecycle outcomes

When all instalments of an account are `Paid`, Finance SHALL publish `OrderPaymentCompleted`; when payment for the account fails terminally, Finance SHALL publish `OrderPaymentFailed`. Both events SHALL carry `TenantId` and `OrderId` so the Order saga can correlate them.

#### Scenario: Final instalment completes the account

- **WHEN** the last outstanding instalment of an account is marked `Paid`
- **THEN** the account transitions to `Completed`
- **AND** `OrderPaymentCompleted` is published for that `OrderId`

#### Scenario: Terminal failure releases the order

- **WHEN** an account's payment is marked terminally failed
- **THEN** the account transitions to `Failed`
- **AND** `OrderPaymentFailed` is published for that `OrderId`
