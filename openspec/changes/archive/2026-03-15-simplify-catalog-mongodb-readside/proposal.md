## Why

The Catalog MongoDB read side (`EShop.Catalog.ReadModels.MongoDb`) has grown complex and difficult to maintain. It uses the raw MongoDB driver directly with a custom generic repository (`MongoRepositoryBase<T>`), custom `IDocument` abstractions, hand-rolled collection mapping (`[MongoCollection]` attribute), and manual idempotency via a bespoke `InboxMessage` collection — all tightly coupled together. This makes it hard for new team members to onboard, difficult to test, and expensive to extend when new read models (e.g., Products) are added.

Additionally, **multi-tenancy is not enforced at the data access layer** — the `TenantId` field on read models exists but is never automatically filtered in queries, meaning all categories are returned regardless of tenant context. This is a data isolation gap that must be addressed.

Migrating to **EF Core with the MongoDB provider** brings a familiar, standardized data access layer that integrates natively with JsonApiDotNetCore, supports dependency inversion through `DbContext` and repository abstractions, and provides a clear seam for injecting tenant-scoped filtering. Combined with a clean project structure following SOLID principles, this will make the read side significantly simpler to maintain and extend.

## What Changes

- **BREAKING**: Remove the raw MongoDB driver repository layer (`IMongoRepositoryBase<T>`, `MongoRepositoryBase<T>`, `IDocument`, `MongoCollectionAttribute`) and replace with EF Core `DbContext` + MongoDB provider
- **BREAKING**: Replace `JsonApiDotNetCore.MongoDb` package with `JsonApiDotNetCore` (EF Core integration) — this changes how JADNC discovers and queries resources
- Introduce a clean **repository pattern** with interfaces in a domain/application layer and implementations in an infrastructure layer, following **Dependency Inversion Principle**
- Introduce **EF Core DbContext for MongoDB** as the single data access abstraction, configured per-tenant
- Implement **automatic multi-tenancy filtering** at the DbContext/repository level using JsonApiDotNetCore's multi-tenancy pattern (global query filters scoped by `TenantId`), so all read queries are tenant-isolated by default
- Restructure the project into a well-organized layered layout separating: Models, Persistence (DbContext + repositories), API (controllers), Event Consumers, and Bootstrapping
- Replace the manual MongoDB `InboxMessage` idempotency with EF Core–compatible idempotency using the shared `IInboxDbContext` pattern already established in the write side
- Prepare the project structure so new read models (e.g., Product) can be added with minimal boilerplate

## Capabilities

### New Capabilities

- `catalog-read-persistence`: EF Core MongoDB DbContext, entity configurations, and repository pattern with dependency inversion for the Catalog read side
- `catalog-read-multi-tenancy`: Automatic tenant-scoped query filtering at the DbContext level integrated with JsonApiDotNetCore's multi-tenancy support
- `catalog-read-project-structure`: Clean layered project organization (Models / Persistence / API / Consumers / Bootstrapping) following SOLID principles

### Modified Capabilities

_(No existing specs in `openspec/specs/` are affected — all changes are scoped to the Catalog read side project which has no prior specs.)_

## Impact

- **Code**: `EShop.Catalog.ReadModels.MongoDb` — full restructure of data access, models, and bootstrapping
- **Dependencies**:
  - Remove: `MongoDB.Driver` (direct usage), `JsonApiDotNetCore.MongoDb`
  - Add: `MongoDB.EntityFrameworkCore`, `JsonApiDotNetCore` (EF Core flavor)
- **APIs**: JSON:API endpoints remain functionally identical (GET categories with filtering/sorting/pagination) but responses will now be automatically scoped to the requesting tenant
- **Shared projects**: May need minor extensions to `EShop.Shared.JsonApi` for MongoDB EF Core registration helpers, and `EShop.Shared.EventBus` for MongoDB-backed inbox integration
- **Event consumers**: `CategoryCreatedConsumer` / `CategoryUpdatedConsumer` will use EF Core `DbContext` instead of raw `IMongoRepositoryBase<T>` for projections
- **Downstream risk**: Existing integration tests in `EShop.Catalog.Tests` that target MongoDB read models will need updating to use EF Core test infrastructure
- **No impact** on: Catalog write side (`EShop.Catalog.Application`), other microservices, integration event contracts, or RabbitMQ message formats
