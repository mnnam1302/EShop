## Context

The Catalog service uses CQRS with two separate deployable units:

- **Write side** (`EShop.Catalog.Application`): PostgreSQL event store, command handlers, aggregates
- **Read side** (`EShop.Catalog.ReadModels.MongoDb`): MongoDB projections, MassTransit consumers, JsonApi query controllers

In production these are two separate ASP.NET processes connected by RabbitMQ. The existing BDD test project (`EShop.Catalog.Tests`) only wires the write side — it uses PostgreSQL Testcontainers, in-memory MassTransit, and registers consumers from `Application` assembly only. The `Then` steps that should verify projected data are unimplemented (`PendingStepException`).

The Tenancy and Authorization test projects follow an established pattern: single `TestServer`, `PostgreSqlTestDatabase`, `TestConsumeObserver.WaitForQuietAsync()` for async settling, and `ScenarioHooks` for lifecycle management. This design extends that pattern to support **multiple backing stores** (PostgreSQL + MongoDB, and eventually Elasticsearch).

## Goals / Non-Goals

**Goals:**
- Wire both write-side and read-side services into a **single composite test host** so the in-memory MassTransit bus connects them without network transport
- Add MongoDB Testcontainers alongside the existing PostgreSQL Testcontainer with proper lifecycle management (start once per test run, clean per scenario)
- Register ReadModels.MongoDb consumers in the same MassTransit in-memory bus so integration events flow end-to-end within the test process
- Implement `Then` step verification by querying MongoDB directly via `IMongoRepositoryBase<Category>`
- Design the infrastructure so adding a third read store (Elasticsearch) later requires only: a new Testcontainer, new DI registrations, and consumer registration — no restructuring

**Non-Goals:**
- Testing the JsonApi read API layer (GET endpoints) — this tests the projection pipeline, not HTTP serialization
- Two-process test topology — production faithfulness is not worth the complexity for BDD tests
- Changes to production code — this is test infrastructure only
- Elasticsearch integration — future ticket, but the design accommodates it

## Decisions

### Decision 1: Single Composite Test Host

**Choice**: Wire both `Catalog.Application` (write) and `Catalog.ReadModels.MongoDb` (read) into one `TestServer`.

**Why over two separate test hosts**: MassTransit's in-memory transport naturally connects all consumers registered in the same bus. Two separate `TestServer` instances would require a shared external bus, complex lifecycle coordination, and add no meaningful test coverage. The existing Tenancy/Authorization pattern uses a single host and it works well.

**Trade-off**: Consumers from both assemblies share one DI container. A read-side consumer could theoretically resolve write-side services. This is acceptable because test DI is not a production boundary — the real service separation is enforced by the project structure and deployment topology.

### Decision 2: MongoDB Testcontainers with Per-Scenario Database Isolation

**Choice**: One `MongoDbContainer` started in `[BeforeTestRun]` (shared across all scenarios for speed), with a **unique database name per scenario** created in `[BeforeScenario]` and dropped in `[AfterScenario]`.

**Why not per-scenario container**: Starting a MongoDB container takes 2-5 seconds. Starting once and switching databases is ~0ms. This matches the PostgreSQL pattern already in use (`PostgreSqlTestDatabase` creates unique databases on a shared container).

**Database naming**: `catalog_test_{Guid:N}` — guarantees no collision if scenarios run in parallel.

**Implementation**: Override `IMongoDbSettings` in test DI to point at the Testcontainer's connection string and the per-scenario database name. The existing `MongoRepositoryBase` resolves `IMongoDatabase` from DI, so swapping the settings is sufficient.

### Decision 3: Consumer Registration from Both Assemblies

**Choice**: Register MassTransit consumers from both assemblies in the in-memory bus configuration:

```
cfg.AddConsumers(Application.AssemblyReference.Assembly);
cfg.AddConsumers(ReadModels.MongoDb.AssemblyReference.Assembly);
```

Both sets of consumers are wired to a single `ReceiveEndpoint("test_queue", ...)`. The `TestConsumeObserver` already tracks all consumer activity regardless of source assembly.

**Why not separate endpoints**: In-memory transport doesn't need separate queues for routing. All consumers in one endpoint simplifies configuration and the `WaitForQuietAsync()` mechanism already handles cascading (write-side publish → read-side consume).

### Decision 4: Direct MongoDB Query for Verification (not JsonApi GET)

**Choice**: `Then` steps resolve `IMongoRepositoryBase<Category>` from DI and query MongoDB directly.

**Why not HTTP GET via JsonApi**: The intent is to verify the projection pipeline (event → consumer → MongoDB document). Adding the JsonApi controller layer would test additional concerns (serialization, routing, resource definitions) that belong in separate read-API tests. Direct queries isolate the projection behavior.

**How**: StepContext gets `IMongoRepositoryBase<Category>` from `ApiContext.ServiceProvider` and queries by `Reference` or `DocumentId`.

### Decision 5: Mediator Registration from ReadModels Assembly

**Choice**: Register the `IMediator` with command handlers from **both** assemblies. The ReadModels assembly has its own `IMediator` registration (`services.AddMediator(ReadModels.AssemblyReference.Assembly)`), but in the composite test host we need a single mediator that knows about handlers from both sides.

**Implementation**: Call `AddMediator` with both assemblies, or register them sequentially. The `IMediator` implementation scans registered assemblies for `ICommandHandler<T>` implementations.

### Decision 6: MongoDB Test Database Helper

**Choice**: Create a `MongoDbTestDatabase` helper class (analogous to `PostgreSqlTestDatabase`) that encapsulates:
- Container reference
- Per-scenario database name generation
- Connection string + database name accessors
- `DropAsync()` for cleanup (drops the per-scenario database)

This keeps `ScenarioHooks.cs` clean and follows the established pattern.

## Risks / Trade-offs

**[Risk] Shared DI Container Between Write and Read Sides**
→ Mitigation: Acceptable for tests. Production isolation is enforced by project boundaries and deployment topology, not by DI scope.

**[Risk] MongoDB container port conflict**
→ Mitigation: Use random port binding (Testcontainers default) rather than fixed port. The PostgreSQL container uses fixed port 36200 which could conflict — MongoDB should use dynamic ports.

**[Risk] MassTransit consumer registration order matters for mediator resolution**
→ Mitigation: Register consumers from both assemblies before configuring endpoints. Order of `AddConsumers` calls is additive and order-independent.

**[Risk] Idempotent consumer requires MongoDB InboxMessage collection**
→ Mitigation: The `IdempotentConsumer` in ReadModels uses `IMongoRepositoryBase<InboxMessage>` which is already registered when we add the MongoDB persistence services. No special setup needed — MongoDB creates collections on first write.

**[Trade-off] Test speed: adding MongoDB container adds ~3-5s to test run startup**
→ Accepted. One-time cost per test run. Per-scenario cost is negligible (database create/drop is fast in MongoDB).

**[Trade-off] Test project now depends on ReadModels.MongoDb project**
→ Accepted. This is the minimal coupling needed to test the full CQRS loop. The alternative (reimplementing consumers in tests) would be worse.
