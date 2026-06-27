## ADDED Requirements

### Requirement: Supported payment frequencies

The system SHALL support the payment frequencies `OneOff`, `Monthly`, `Quarterly`, and `Annually`. Each frequency defines the number of instalments and the interval between their due dates.

#### Scenario: Frequency determines instalment count and interval

- **WHEN** a schedule is generated for a 12-month term
- **THEN** `OneOff` produces 1 instalment, `Monthly` produces 12 instalments one month apart, `Quarterly` produces 4 instalments three months apart, and `Annually` produces 1 instalment

#### Scenario: Unknown frequency is rejected

- **WHEN** a schedule is requested with a frequency outside the supported set
- **THEN** schedule generation fails with a domain error and no instalments are created

### Requirement: Instalment amounts sum to the total

The schedule calculator SHALL divide the order total evenly across instalments using the currency's minor-unit precision, and SHALL absorb any rounding remainder into the final instalment so that the sum of all instalment amounts equals the order total exactly.

#### Scenario: Even division

- **WHEN** a total of 120.00 is split into 4 quarterly instalments
- **THEN** each instalment is 30.00 and the four amounts sum to 120.00

#### Scenario: Remainder absorbed by final instalment

- **WHEN** a total of 100.00 is split into 3 instalments
- **THEN** the first two instalments are 33.33, the final instalment is 33.34, and the three amounts sum to 100.00

#### Scenario: Zero or negative total is rejected

- **WHEN** a schedule is requested for a total that is zero or negative
- **THEN** schedule generation fails with a domain error

### Requirement: Instalment due dates

The first instalment SHALL be due on the schedule start date; each subsequent instalment SHALL be due at the frequency interval after the previous one.

#### Scenario: Monthly due dates advance by one month

- **WHEN** a monthly schedule starts on 2026-01-15 with 3 instalments
- **THEN** the instalments are due 2026-01-15, 2026-02-15, and 2026-03-15

### Requirement: Instalment state transitions

Each `Instalment` SHALL move through the states `Pending` → `Booked` → `Paid`, with `Booked` → `Failed` permitted on booking/payment failure. The aggregate SHALL reject transitions that are not permitted from the current state.

#### Scenario: Valid progression

- **WHEN** a `Pending` instalment is booked and then paid
- **THEN** it ends in state `Paid`

#### Scenario: Illegal transition rejected

- **WHEN** an attempt is made to mark a `Pending` instalment `Paid` without first booking it
- **THEN** the aggregate rejects the transition with a domain error
