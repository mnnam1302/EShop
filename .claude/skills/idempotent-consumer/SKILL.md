---
name: idempotent-consumer
description: >
  Solution design for making message consumers idempotent under
  at-least-once delivery guarantees. Use when building event consumers
  that must not produce duplicate side-effects (accounting transactions,
  state changes, notifications) when the same message is redelivered by
  RabbitMQ, Kafka, SQS, Azure Service Bus, or any at-least-once broker.
---

# Idempotent Consumer Pattern (Inbox)

Ensures a message consumer produces the same outcome whether it processes
a message once or ten times — critical in finance where a duplicate event
means a duplicate accounting entry and real money impact.

## The problem

Message brokers guarantee **at-least-once delivery**, not exactly-once.
A consumer can receive the same message multiple times when:

- Consumer processes the message, crashes **before** acknowledging it
- Broker times out waiting for acknowledgment and redelivers
- Network partition causes the broker to assume the consumer is dead

Exactly-once **delivery** is impossible over a network. What you can
achieve is exactly-once **effect**: processing a message twice produces
the same final state as processing it once.

## Core insight

The consumer must make **dedup check** and **business side-effect**
atomic — in the same database transaction. If they are separate
operations, a crash between them creates either a missed message or a
duplicate effect.

## Inbox pattern

Store a record of each processed message in an `InboxMessages` table
alongside the business write, in a single DB transaction.

### Schema

```sql
CREATE TABLE InboxMessages (
    Id              BIGINT IDENTITY PRIMARY KEY,
    MessageId       UNIQUEIDENTIFIER NOT NULL,
    ConsumerName    NVARCHAR(256)    NOT NULL,
    ProcessedAt     DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT UQ_Inbox_Consumer_Message
        UNIQUE (ConsumerName, MessageId)
);
```

**Why composite key `(ConsumerName, MessageId)`:** multiple consumers may
process the same message independently. Each consumer tracks its own
dedup state. A single-column unique on `MessageId` would block unrelated
consumers from processing a shared event.

## Consumer flow

```
 1  Message arrives from broker

 2  Quick check: SELECT WHERE MessageId = @id AND ConsumerName = @name
    → Found → ACK message → return early (already processed)

 3  BEGIN TRANSACTION

 4      INSERT INTO InboxMessages (MessageId, ConsumerName)
        → Unique constraint violation → CATCH → ACK → return
          (concurrent duplicate that passed the SELECT in step 2)

 5      Execute business logic (e.g., INSERT AccountingTransaction)

 6  COMMIT TRANSACTION

 7  ACK message to broker
```

### Why each step exists

**Step 2 (SELECT) is optimization, NOT the safety mechanism.** Under
Read Committed (default for SQL Server and PostgreSQL), two concurrent
consumers processing the same `MessageId` both execute SELECT, both see
"not found" (because neither has committed yet), and both proceed to
INSERT. The SELECT only saves work when a message arrives long after
the first processing.

**Step 4 (INSERT with unique constraint) is the actual guard.** When
two concurrent INSERTs race, the database serializes them at the index
level. One commits; the other gets a duplicate-key violation. This works
regardless of isolation level.

**Step 4 catch → ACK (not NACK).** On duplicate-key violation, the
consumer MUST acknowledge the message. If it NACKs instead, the broker
redelivers the message, the consumer hits the same violation, NACKs
again — infinite retry loop.

**Steps 4 + 5 in one transaction (atomicity).** If the consumer crashes
after step 5 but before step 6 (COMMIT), the entire transaction rolls
back — both the inbox record and the business write disappear. The
broker redelivers the message, and the consumer processes it cleanly
as if the first attempt never happened. No duplicate, no data loss.

## Failure mode analysis

```
Scenario                          What happens                     Result
─────────────────────────────────────────────────────────────────────────────
Happy path                       SELECT(miss)→TX→INSERT+BIZ→      Correct
                                 COMMIT→ACK

Redelivery (after prior commit)  SELECT(hit)→ACK→return           Correct
                                                                   (skipped)

Concurrent duplicate             SELECT(miss)→TX→INSERT→           Correct
(two messages, same ID)          unique violation→CATCH→ACK        (one wins)

Crash before COMMIT              TX rolls back (inbox + biz)→      Correct
                                 broker redelivers→                (clean
                                 processed fresh                    retry)

Crash after COMMIT,              Message redelivered→              Correct
before ACK                       SELECT(hit)→ACK→return            (skipped)

DB connection lost mid-TX        TX rolls back (implicit)→         Correct
                                 NACK (no ACK sent)→               (clean
                                 broker redelivers                  retry)
```

Every scenario ends in a correct state. The key property: there is **no
window** where the business write exists without the inbox record, or
vice versa, because they share one transaction boundary.

## Integration with MassTransit / NServiceBus

Most .NET messaging frameworks have built-in inbox support:

**MassTransit:** enable via `cfg.UseInMemoryInboxOutbox()` (in-memory)
or `cfg.UseEntityFrameworkOutbox()` (durable, recommended for finance).
The framework handles dedup, transactional outbox, and retry
automatically.

**NServiceBus:** enable via `endpointConfiguration.EnableOutbox()`.
Similar transactional guarantee using the Outbox pattern.

**When to build custom (as described above):**
- Framework inbox doesn't support your DB provider
- You need composite dedup keys beyond `MessageId`
- You need audit fields (processed timestamp, consumer version)
- You want explicit control over retry/error handling behavior

## Key rules

1. Dedup and business write MUST share one DB transaction. Separate
   operations = crash gap = duplicates or data loss.
2. Unique constraint is the guard, SELECT is the optimization.
   Never rely on SELECT alone for correctness.
3. Duplicate-key violation → ACK, never NACK. NACK causes infinite
   retry loops.
4. ConsumerName in the composite key. Multiple consumers processing
   the same event must track dedup independently.
5. Idempotency key comes from the producer. Use `MessageId` or
   `CorrelationId` defined upstream — never generate a new ID in the
   consumer (defeats dedup on redelivery).
6. Keep business logic inside the transaction boundary. Any side-effect
   outside the transaction (HTTP calls, file writes) cannot roll back
   and needs its own idempotency strategy.