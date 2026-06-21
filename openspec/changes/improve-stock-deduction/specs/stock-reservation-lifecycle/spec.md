## ADDED Requirements

### Requirement: Reservation state machine

The system SHALL model each order's hold as a `Reservation` with states `Pending`, `Confirmed`, `Released`, and `Expired`, where `Pending` is the only non-terminal state and transitions out of `Pending` are mutually exclusive.

#### Scenario: A new reservation starts Pending
- **WHEN** stock is deducted for an order
- **THEN** the reservation SHALL be created in `Pending` state with an expiry 15 minutes in the future

#### Scenario: Terminal states are not re-transitioned
- **WHEN** a transition is attempted on a reservation already in `Confirmed`, `Released`, or `Expired`
- **THEN** the system SHALL treat the transition as a no-op and SHALL NOT change stock again

### Requirement: Payment-gated confirmation

The system SHALL confirm a `Pending` reservation only on payment success, via `ConfirmReservationCommand`, moving it to `Confirmed` without changing stock levels (stock was already deducted at placement).

#### Scenario: Payment success confirms the hold
- **WHEN** a `ConfirmReservationCommand` is received for a `Pending` reservation
- **THEN** the reservation SHALL move to `Confirmed` and `stock_available` SHALL remain unchanged

#### Scenario: Confirmed reservations are ignored by the sweeper
- **WHEN** the TTL sweeper runs after a reservation is `Confirmed`
- **THEN** the sweeper SHALL NOT expire or release that reservation

### Requirement: Release returns stock idempotently

The system SHALL return reserved stock to the available pool when a `ReleaseReservationCommand` is received for stock-fail, order rejection, cancellation, or payment failure, adding back each `ReservationItem` quantity, and SHALL be safe to apply more than once.

#### Scenario: Cancellation restores stock per variant
- **WHEN** a `ReleaseReservationCommand` is received for a `Pending` reservation
- **THEN** the system SHALL increment `stock_available` by each `ReservationItem` quantity, mark the reservation `Released`, and release the corresponding Redis quantities

#### Scenario: Duplicate release is a no-op
- **WHEN** a release is received for a reservation already in `Released` or `Expired`
- **THEN** the system SHALL NOT add stock back a second time

### Requirement: TTL sweeper expires abandoned reservations

The system SHALL run a background sweeper that finds `Pending` reservations whose expiry has passed, returns their stock to the available pool, and marks them `Expired`.

#### Scenario: Unpaid reservation past 15 minutes is reclaimed
- **WHEN** a reservation remains `Pending` for longer than 15 minutes
- **THEN** the sweeper SHALL increment `stock_available` by each `ReservationItem` quantity, mark the reservation `Expired`, and release the corresponding Redis quantities

#### Scenario: Cancellation racing expiry returns stock once
- **WHEN** a `ReleaseReservationCommand` and the TTL sweeper both target the same `Pending` reservation
- **THEN** exactly one of them SHALL apply the add-back and the other SHALL be a no-op
