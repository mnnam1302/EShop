## Why

The Catalog BDD tests currently only verify the write side (Command → PostgreSQL EventStore → integration event published). The read side (MongoDB projections) is completely untested — the `Then` step throws `PendingStepException`. This means the full CQRS loop (Command → Domain Event → Integration Event → Consumer → MongoDB Projection) has zero automated coverage. As the Catalog service grows with Products, Updates, and eventually Elasticsearch-based search, the test infrastructure must support verifying projections across multiple read stores to catch regressions early and accelerate future ticket delivery.

## What Changes

- Add **MongoDB Testcontainers** to the Catalog test project alongside the existing PostgreSQL Testcontainer
- Register **ReadModels.MongoDb consumers and handlers** in the in-memory MassTransit bus within the test host (single composite test host approach)
- Implement **end-to-end BDD verification** for the CreateCategory flow: HTTP POST → EventStore → Integration Event → Consumer → MongoDB Projection → assertion
- Implement **end-to-end BDD verification** for the UpdateCategory flow through the same pipeline
- Design the test infrastructure as a **reusable pattern** so future read model stores (e.g., Elasticsearch for product search) can be wired into the same composite test host without restructuring
- Complete the pending `Then` steps with direct MongoDB queries to verify projected data

## Capabilities

### New Capabilities
- `catalog-cqrs-test-infra`: Test infrastructure for wiring multiple read model backends (MongoDB now, Elasticsearch later) into a single composite BDD test host with Testcontainers lifecycle management
- `catalog-bdd-read-model-verification`: BDD step definitions and step contexts for verifying that integration events produce correct read model projections in MongoDB, including idempotency checks

### Modified Capabilities
_None — no existing spec-level requirements are changing._

## Impact

- **Test project**: `EShop.Catalog.Tests` — new project reference to `EShop.Catalog.ReadModels.MongoDb`, new package dependency on `Testcontainers.MongoDb`
- **Test setup**: `ScenarioHooks.cs`, `ServiceCollectionExtensions.cs`, `TestStartup.cs`, `ApiContext.cs` — MongoDB container lifecycle, DI registration for read model services, consumer wiring
- **BDD scenarios**: `CreateCategory.feature`, `Steps.cs`, `StepContext.cs` — completed verification steps
- **No production code changes** — this change is test-only
- **Future extensibility**: The composite test host pattern established here will support adding Elasticsearch Testcontainers for product search without restructuring the test infrastructure
