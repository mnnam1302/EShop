## ADDED Requirements

### Requirement: Asynchronous booking after scheduling
The system SHALL book an account's scheduled payments to the tenant's accounting provider asynchronously, decoupled from the Order saga schedule reply, so that external-provider latency does not delay order acceptance. Booking SHALL be triggered once payments have been scheduled for an account.

#### Scenario: Booking is triggered after payments are scheduled
- **WHEN** an account's payments have been scheduled
- **THEN** the system initiates booking of those payments to the tenant's provider without blocking the saga schedule reply

#### Scenario: Order acceptance does not wait on booking
- **WHEN** the accounting provider is slow or unavailable at the time payments are scheduled
- **THEN** the saga schedule reply is unaffected
- **AND** booking is attempted independently and retried on failure

### Requirement: Idempotent find-then-create booking
The system SHALL ensure each payment is booked at most once at the provider, even under message redelivery or retries. For each payment it SHALL: short-circuit when the payment already has a recorded external booking reference; otherwise attempt to find an existing booking at the provider; and create a new booking only when none is found. A successful find or create SHALL record the external reference on the payment, and recording an already-booked payment SHALL have no additional effect.

#### Scenario: Duplicate delivery does not double-book
- **WHEN** the booking trigger for an account is delivered more than once
- **THEN** payments that already have a recorded external reference are not re-created at the provider

#### Scenario: Existing remote booking is adopted, not duplicated
- **WHEN** booking runs for a payment that has no recorded external reference but already exists at the provider
- **THEN** the system adopts the existing provider booking and records its reference rather than creating a new one

#### Scenario: New payment is created once
- **WHEN** booking runs for a payment with no recorded external reference and no existing provider booking
- **THEN** the system creates the booking at the provider exactly once and records the returned reference

### Requirement: External reference recorded on the account
The system SHALL record the provider's external reference on the account by invoking the account's booking behaviour, producing the corresponding domain event for each successfully booked payment.

#### Scenario: Account reflects a successful booking
- **WHEN** a payment is successfully booked at the provider
- **THEN** the system records the external reference against that payment on the account
- **AND** a payment-booked domain event is raised

### Requirement: Failure isolation and retry
The system SHALL treat booking failures as recoverable: a failed payment SHALL publish a booking-failure notification and remain eligible for retry so the ledger converges on a subsequent run. A failure booking one payment MUST NOT abort booking of the account's other payments.

#### Scenario: Transient failure is retried on the next run
- **WHEN** booking a payment fails due to a transient provider or communication error
- **THEN** the system publishes a booking-failure notification for that payment
- **AND** the payment remains eligible to be booked on a later run

#### Scenario: One failure does not block the rest
- **WHEN** one payment in an account fails to book while others succeed
- **THEN** the succeeding payments are booked and recorded
- **AND** only the failing payment is reported as failed

### Requirement: No-op for unconfigured tenants
The system SHALL perform no external booking for a tenant that has not registered an accounting provider, leaving the account's scheduling outcome unchanged.

#### Scenario: Unconfigured tenant books nothing
- **WHEN** payments are scheduled for an account belonging to a tenant with no registered provider
- **THEN** the system attempts no external call
- **AND** the account's payments remain in their scheduled state
