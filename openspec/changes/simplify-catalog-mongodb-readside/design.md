## Context

The Catalog read side (`EShop.Catalog.ReadModels.MongoDb`) is a standalone ASP.NET web API that consumes integration events from the Catalog write side via MassTransit/RabbitMQ and projects them into MongoDB read models, served through JsonApiDotNetCore (JADNC) endpoints.

**Current state:**

- Uses `MongoDB.Driver` directly with a custom `MongoRepositoryBase<TDocument>` generic repository, `IDocument` base interface (wrapping JADNC's `IMongoIdentifiable`), and `[MongoCollection]` attribute for collection mapping.
- JADNC is registered via `JsonApiDotNetCore.MongoDb` package with `MongoRepository<,>` as the resource repository.
- Idempotency is handled through a bespoke `IdempotentConsumer<T>` base class that checks/inserts `InboxMessage` documents via the same `IMongoRepositoryBase<InboxMessage>`.
- `TenantId` is stored on read models but **never filtered** — all tenants' data is returned from JADNC queries.
- The write side (`EShop.Catalog.Application`) already uses EF Core + PostgreSQL with the shared `RepositoryBase<TDbContext, TEntity, TKey>`, `IInboxDbContext`, `IScoped`/`IExcludedFromScoping` patterns, and PostgreSQL Row-Level Security for tenant isolation.

**Constraints:**

- The solution uses centrally managed package versions via `Directory.Packages.props` (EF Core 8.0.11, MongoDB.Driver 3.3.0, JADNC.MongoDb 5.10.0).
- `MongoDB.EntityFrameworkCore` is not yet in the solution and must be added.
- The shared `RepositoryBase` requires entities to implement `IEntityBase<TKey>` — read models must conform.
- JADNC resources require `[Resource]` and `[Attr]` annotations and a JADNC-compatible `Id` property.
- Multi-tenancy on the write side relies on PostgreSQL RLS via `PostgresMultiTenantConnectionInterceptor` — this pattern cannot be reused for MongoDB and needs an EF Core global query filter approach instead.

## Goals / Non-Goals

**Goals:**

- Replace raw MongoDB driver usage with EF Core MongoDB provider (`MongoDB.EntityFrameworkCore`) as the sole data access layer
- Integrate JADNC with EF Core (standard `JsonApiDotNetCore` package) instead of the MongoDB-specific `JsonApiDotNetCore.MongoDb` package
- Implement automatic tenant-scoped query filtering using EF Core global query filters, integrated with JADNC's multi-tenancy pattern
- Apply the repository pattern with dependency inversion: interfaces alongside the domain models, implementations in a persistence layer
- Align with the write side's shared `IInboxDbContext` pattern for idempotent event consumption
- Restructure the project into clean, maintainable layers following SOLID principles
- Make adding new read models (e.g., Product) a minimal-boilerplate exercise

**Non-Goals:**

- Migrating the write side away from PostgreSQL or changing the event sourcing architecture
- Changing integration event contracts or RabbitMQ topology
- Adding new JSON:API endpoints (e.g., Product read endpoints) — this change prepares the structure only
- Implementing ring-fenced scoping (`IRingFenced`) for the read side — tenant-level isolation via `IScoped` is sufficient
- Replacing MassTransit or the consumer-based projection pattern

## Decisions

### D1: Use `MongoDB.EntityFrameworkCore` as the MongoDB EF Core Provider

**Choice:** Use the official MongoDB EF Core provider (`MongoDB.EntityFrameworkCore`) instead of continuing with raw `MongoDB.Driver`.

**Rationale:** EF Core provides a familiar `DbContext` abstraction that aligns with the rest of the solution (write side already uses EF Core + PostgreSQL). It enables global query filters for multi-tenancy, integrates with JADNC's `EntityFrameworkCoreRepository<,>`, and supports `IEntityTypeConfiguration<T>` for clean entity mapping. The MongoDB EF Core provider supports .NET 8 and EF Core 8.x.

**Alternatives considered:**
- *Keep raw MongoDB.Driver with improved abstractions:* Lower risk, but perpetuates a divergent data access pattern and cannot leverage EF Core global query filters for automatic tenant scoping.
- *Use a third-party ODM (e.g., MongoFramework):* Less community adoption and not compatible with JADNC's built-in EF Core integration.

### D2: Single `CatalogReadDbContext` with Global Query Filters for Multi-Tenancy

**Choice:** Create a `CatalogReadDbContext : DbContext` that:
- Exposes `DbSet<Category>` (and future `DbSet<Product>`, etc.)
- Exposes `DbSet<InboxMessage>` (implementing `IInboxDbContext`)
- Applies a global query filter `entity.TenantId == _tenantId` on all `IScoped` entities
- Resolves `_tenantId` from the scoped `ITenantProvider` (injected via JADNC's multi-tenancy pattern)

**Rationale:** This mirrors the write side's `CatalogDbContext` structure. Global query filters ensure every JADNC query is automatically tenant-scoped without per-controller filtering. `InboxMessage` implements `IExcludedFromScoping` so it's unfiltered (consistent with the write side).

**Alternatives considered:**
- *Per-tenant database approach:* More isolated but increases operational complexity (many MongoDB databases). Overkill for the current scale.
- *Manual filtering in controllers/repositories:* Error-prone — forgetting a filter leaks cross-tenant data.

### D3: JADNC Multi-Tenancy via `IResourceDefinition` and `ITenantProvider`

**Choice:** Follow the [JADNC multi-tenancy pattern](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/docs/usage/advanced/multi-tenancy.md):
1. Register an `ITenantProvider` service that resolves the current tenant from the HTTP request context (via the existing `IUserDetailsProvider.AuthenticatedUser.TenantId`)
2. The `CatalogReadDbContext` uses the `ITenantProvider` to apply global query filters
3. JADNC's `EntityFrameworkCoreRepository<TResource, TId>` automatically queries through the `DbContext`, which applies the filter transparently

**Rationale:** This is the documented, supported approach for JADNC multi-tenancy. It does not require custom repository implementations — JADNC's EF Core integration handles all query/filter/sort/pagination by delegating to the `DbContext`.

**Alternatives considered:**
- *Custom JADNC repository overriding `GetAll()`:* Fragile — must override every query entry point and maintain it when JADNC upgrades.
- *Middleware-based query rewriting:* Non-standard, complex, and easy to misconfigure.

### D4: Repository Pattern with Dependency Inversion

**Choice:** Define repository interfaces alongside the read models and implement them in the persistence layer:
- `ICategoryReadRepository` — interface for projection handlers to upsert/query categories
- `CategoryReadRepository : RepositoryBase<CatalogReadDbContext, Category, string>` — implementation using the shared `RepositoryBase` pattern

JADNC endpoints will **not** use these repositories — JADNC queries go directly through its own `EntityFrameworkCoreRepository<,>` which uses the `CatalogReadDbContext`. The custom repositories are only for event consumer projection handlers.

**Rationale:** Separates the projection concern (event handlers inserting/updating data) from the query concern (JADNC serving API requests). Both share the same `CatalogReadDbContext`, so tenant filtering and data consistency are guaranteed. This follows the Dependency Inversion Principle — handlers depend on abstractions, not on the DbContext directly.

**Alternatives considered:**
- *Use DbContext directly in handlers:* Simpler but creates tight coupling and makes unit testing harder.
- *Single repository for both JADNC and projections:* JADNC already has its own repository abstraction — wrapping it in another layer adds indirection without value.

### D5: Read Model Entities Implement `IEntityBase<string>` and `IScoped`

**Choice:** Read model entities (e.g., `Category`) will:
- Implement `IEntityBase<string>` with `Id` as the MongoDB `_id` field (string type, matching current JADNC expectations)
- Implement `IScoped` with `TenantId` and `Scope` properties
- Retain JADNC `[Resource]` and `[Attr]` annotations
- Remove `IDocument`, `Document`, `IMongoIdentifiable` base classes
- Keep `DocumentId` (Guid — the write-side aggregate ID) as a separate property from `Id`

**Rationale:** `IEntityBase<string>` enables use of the shared `RepositoryBase`. `IScoped` enables global query filter application. The `Id` (string) is the MongoDB `_id` for JADNC compatibility; `DocumentId` (Guid) links back to the aggregate for idempotency checks.

**Alternatives considered:**
- *Use `Guid` as Id:* JADNC with MongoDB typically uses string IDs (ObjectId or hex). Changing to Guid would require custom serialization.
- *Drop `DocumentId`:* This is needed for projection idempotency (checking if an aggregate's projection already exists).

### D6: Project Structure — Layered Organization

**Choice:** Reorganize `EShop.Catalog.ReadModels.MongoDb` into:

```
EShop.Catalog.ReadModels.MongoDb/
├── Program.cs
├── Startup.cs
├── Bootstrapping/
│   ├── ServiceCollectionExtensions.cs  (orchestrates all registrations)
│   └── SwaggerExtensions.cs
├── Models/
│   ├── Category.cs                     (JADNC resource + IScoped + IEntityBase)
│   ├── ICategoryReadRepository.cs      (repository interface)
│   └── InboxMessage.cs                 (IExcludedFromScoping + IEntityBase)
├── Persistence/
│   ├── CatalogReadDbContext.cs          (EF Core DbContext + IInboxDbContext)
│   ├── CategoryReadRepository.cs        (RepositoryBase implementation)
│   └── EntityConfigurations/
│       ├── CategoryEntityConfiguration.cs
│       └── InboxMessageEntityConfiguration.cs
├── Controllers/
│   └── CategoriesController.cs         (JADNC controller, unchanged interface)
├── Consumers/
│   ├── IdempotentConsumer.cs            (base class, now uses IInboxDbContext)
│   ├── CategoryCreatedConsumer.cs
│   └── CategoryUpdatedConsumer.cs
└── Handlers/
    ├── CreateCategoryProjectionCommandHandler.cs
    └── UpdateCategoryProjectionCommandHandler.cs
```

**Rationale:**
- **Models/**: Contains read model entities and their repository interfaces (dependency inversion — interfaces near the domain, not the persistence layer)
- **Persistence/**: Contains `DbContext`, entity configurations, and repository implementations — the only layer that knows about EF Core/MongoDB specifics
- **Controllers/**: JADNC controllers remain thin wrappers
- **Consumers/**: MassTransit integration event consumers (idempotency + dispatch to handlers)
- **Handlers/**: Projection command handlers (CQRS command handlers that upsert read models)
- **Bootstrapping/**: DI registration orchestration

This structure makes it immediately clear where to add a new read model: define the entity in `Models/`, add a `DbSet` in `Persistence/CatalogReadDbContext.cs`, add an entity configuration, create its consumer and handler, and add the JADNC controller.

**Alternatives considered:**
- *Separate projects (e.g., `EShop.Catalog.ReadModels.Domain` + `EShop.Catalog.ReadModels.Infrastructure`):* Too much ceremony for a read-side projection service — read models are simple DTOs, not rich domain objects.
- *Feature-folder organization (e.g., `Categories/`, `Products/`):* Works for the write side with its complex command/event/aggregate structure, but read-side models are simple enough that layer-based organization is clearer.

### D7: Idempotent Consumer Refactored to Use EF Core

**Choice:** Refactor `IdempotentConsumer<T>` to accept `CatalogReadDbContext` (via `IInboxDbContext`) instead of `IMongoRepositoryBase<InboxMessage>`. The idempotency check and insert will use EF Core operations on `DbSet<InboxMessage>`.

**Rationale:** Aligns with the write side's idempotency approach (shared `InboxMessage` entity + `IInboxDbContext`). The `InboxMessage` entity is already shared from `EShop.Shared.EventBus` — no duplication needed. The consumer saves changes via `DbContext.SaveChangesAsync()` in a single unit of work with the projection.

**Alternatives considered:**
- *Keep InboxMessage in a separate MongoDB collection via raw driver:* Creates two data access paths (EF Core + raw driver) in the same service — defeats the simplification goal.

## Risks / Trade-offs

**[EF Core MongoDB Provider Maturity]** → The `MongoDB.EntityFrameworkCore` provider is newer than the raw driver and may have limitations (e.g., limited LINQ translation, no migration support). **Mitigation:** The read side is query-heavy with simple filters — well within supported LINQ operations. No migrations needed (MongoDB is schemaless). Test thoroughly with JADNC's query translation.

**[JADNC Compatibility]** → Switching from `JsonApiDotNetCore.MongoDb` to `JsonApiDotNetCore` (EF Core) changes the underlying repository. **Mitigation:** JADNC's EF Core integration is the primary supported path and is more mature than the MongoDB-specific package. Verify filtering, sorting, and pagination behavior in integration tests.

**[Global Query Filter Bypass]** → EF Core global query filters can be bypassed with `IgnoreQueryFilters()`. Projection handlers that insert data for a specific tenant must not accidentally query across tenants. **Mitigation:** Projection handlers use repository abstractions that go through the filtered `DbContext`. The `InboxMessage` entity uses `IExcludedFromScoping` and is intentionally unfiltered.

**[Breaking Change]** → Existing clients querying categories without tenant context will now receive empty results (previously they received all tenants' data). **Mitigation:** This is a security improvement — the previous behavior was a data isolation gap. Coordinate with API consumers to ensure proper tenant headers are sent.

**[Unit of Work Scope]** → Combining projection writes and inbox writes in a single `SaveChangesAsync()` call means they succeed or fail together. If MongoDB doesn't support transactions in the EF Core provider, these could be inconsistent. **Mitigation:** Check transaction support. If unavailable, keep the idempotency-first pattern (check inbox → project → mark inbox) to ensure at-least-once processing with idempotent projections.

**[Package Version Alignment]** → `MongoDB.EntityFrameworkCore` must be compatible with EF Core 8.0.11 and MongoDB.Driver 3.3.0. **Mitigation:** Verify NuGet compatibility before implementation. Pin versions in `Directory.Packages.props`.
