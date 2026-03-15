## 1. Package & Dependency Setup

- [x] 1.1 Add `MongoDB.EntityFrameworkCore` package version entry to `Directory.Packages.props` (compatible with EF Core 8.x and MongoDB.Driver 3.x)
- [x] 1.2 Add `JsonApiDotNetCore` (EF Core flavor) package version entry to `Directory.Packages.props` if not already present
- [x] 1.3 Update `EShop.Catalog.ReadModels.MongoDb.csproj`: replace `JsonApiDotNetCore.MongoDb` with `JsonApiDotNetCore`, add `MongoDB.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore`, add project reference to `EShop.Shared.DomainTools`
- [x] 1.4 Remove direct `MongoDB.Driver` package reference from csproj (it will come transitively via `MongoDB.EntityFrameworkCore`)
- [x] 1.5 Verify the solution builds with the updated package references (resolve any version conflicts)

## 2. Project Structure Reorganization

- [x] 2.1 Create `Persistence/` folder and `Persistence/EntityConfigurations/` subfolder
- [x] 2.2 Rename `IntegrationEventConsumers/` folder to `Consumers/`
- [x] 2.3 Move `IdempotentConsumer.cs`, `CategoryCreatedConsumer.cs`, `CategoryUpdatedConsumer.cs` into `Consumers/` and update namespaces
- [x] 2.4 Verify `Models/`, `Controllers/`, `Handlers/`, `Bootstrapping/` folders exist (already present — confirm structure)
- [x] 2.5 Delete `Infrastructure/` folder and all contents (`IMongoRepositoryBase.cs`, `MongoRepositoryBase.cs`, `MongoCollectionAttribute.cs`, `IMongoDbSettings.cs`) after replacements are in place

## 3. Read Model Entity Refactoring

- [x] 3.1 Refactor `Category` entity: implement `IEntityBase<string>` and `IScoped`, add `Id` (string) property, add `Scope` property, retain `DocumentId` (Guid), keep JADNC `[Resource]` and `[Attr]` annotations, remove inheritance from `Document`/`IDocument`/`IMongoIdentifiable`
- [x] 3.2 Remove `IDocument.cs`, `Document.cs` base classes from `Models/`
- [x] 3.3 Remove local `InboxMessage.cs` from `Models/` — the shared `InboxMessage` from `EShop.Shared.EventBus` will be used instead
- [x] 3.4 Create `ICategoryReadRepository` interface in `Models/` extending `IRepositoryBase<Category, string>` from `EShop.Shared.DomainTools`

## 4. Multi-Tenancy Infrastructure

- [x] 4.1 Create `ITenantProvider` interface with `string? TenantId` property (place in `Models/` or a shared location)
- [x] 4.2 Create `TenantProvider` implementation that resolves `TenantId` from `IUserDetailsProvider.AuthenticatedUser.TenantId`
- [x] 4.3 Make `TenantProvider` settable so MassTransit consumers can override the tenant context from integration event payloads
- [x] 4.4 Register `ITenantProvider` / `TenantProvider` as scoped in DI

## 5. EF Core DbContext & Entity Configurations

- [x] 5.1 Create `CatalogReadDbContext` in `Persistence/` extending `DbContext`, implementing `IInboxDbContext`, accepting `DbContextOptions` and `ITenantProvider` in constructor
- [x] 5.2 Add `DbSet<Category>` and `DbSet<InboxMessage>` properties to `CatalogReadDbContext`
- [x] 5.3 Override `OnModelCreating` to apply configurations from assembly and add global query filter for `IScoped` entities (`entity.TenantId == _tenantId`)
- [x] 5.4 Create `CategoryEntityConfiguration` in `Persistence/EntityConfigurations/` implementing `IEntityTypeConfiguration<Category>`, mapping to `Category` MongoDB collection with `Id` as key
- [x] 5.5 Create `InboxMessageEntityConfiguration` in `Persistence/EntityConfigurations/` implementing `IEntityTypeConfiguration<InboxMessage>`, mapping to `InboxMessages` MongoDB collection

## 6. Repository Implementation

- [x] 6.1 Create `CategoryReadRepository` in `Persistence/` extending `RepositoryBase<CatalogReadDbContext, Category, string>`, implementing `ICategoryReadRepository`
- [x] 6.2 Register `ICategoryReadRepository` → `CategoryReadRepository` as scoped service in DI bootstrapping

## 7. Bootstrapping & DI Registration Updates

- [x] 7.1 Refactor `AddMongoDbPersistence()` to register `CatalogReadDbContext` with MongoDB EF Core provider (replace raw `MongoClient`/`IMongoDatabase` singleton setup)
- [x] 7.2 Remove `IMongoDbSettings`, `MongoDbSettings` options registration and `BsonSerializer` configuration
- [x] 7.3 Refactor `AddJsonApiDotNet()` to use `AddJsonApi()` with EF Core integration: replace `AddJsonApiMongoDb()` with `services.AddEntityFrameworkCoreRepository<CatalogReadDbContext>()`, remove `MongoRepository<,>` registrations
- [x] 7.4 Register `ITenantProvider` and repository services in DI
- [x] 7.5 Remove `IMongoRepositoryBase<>` / `MongoRepositoryBase<>` scoped registration

## 8. Consumer Refactoring

- [x] 8.1 Refactor `IdempotentConsumer<T>` base class: replace `IMongoRepositoryBase<InboxMessage>` dependency with `CatalogReadDbContext` (via `IInboxDbContext`), update idempotency check to use `DbSet<InboxMessage>` queries, persist via `SaveChangesAsync()`
- [x] 8.2 Update `CategoryCreatedConsumer` constructor to pass `CatalogReadDbContext` to `IdempotentConsumer` base and inject `IMediator`
- [x] 8.3 Update `CategoryUpdatedConsumer` constructor similarly
- [x] 8.4 Update consumers to set `ITenantProvider.TenantId` from integration event's `TenantId` before dispatching to handler

## 9. Handler Refactoring

- [x] 9.1 Refactor `CreateCategoryProjectionCommandHandler`: replace `IMongoRepositoryBase<Category>` with `ICategoryReadRepository`, use repository `FindSingleAsync()` / `Add()` + `DbContext.SaveChangesAsync()` pattern
- [x] 9.2 Refactor `UpdateCategoryProjectionCommandHandler`: replace `IMongoRepositoryBase<Category>` with `ICategoryReadRepository`, use repository `FindByIdAsync()` / `Update()` + `DbContext.SaveChangesAsync()` pattern
- [x] 9.3 Inject `CatalogReadDbContext` into handlers for `SaveChangesAsync()` calls (or add a `SaveChangesAsync()` method to the repository interface)

## 10. Controller Updates

- [x] 10.1 Update `CategoriesController` to inherit from the JADNC EF Core controller base class (if the base class signature changes) — remove any MongoDB-specific references
- [x] 10.2 Verify authorization attributes (`[RequireFeature]`, `[RequireOneOfPermissions]`) remain intact
- [x] 10.3 Verify controller contains no explicit tenant filtering logic

## 11. Cleanup & Removal

- [x] 11.1 Delete `Infrastructure/Attributes/MongoCollectionAttribute.cs`
- [x] 11.2 Delete `Infrastructure/IMongoDbSettings.cs` and `MongoDbSettings` class
- [x] 11.3 Delete `Infrastructure/IMongoRepositoryBase.cs`
- [x] 11.4 Delete `Infrastructure/Repository/MongoRepositoryBase.cs`
- [x] 11.5 Delete `Models/IDocument.cs` and `Models/Document` base class (if separate file)
- [x] 11.6 Delete local `Models/InboxMessage.cs` (now using shared entity)
- [x] 11.7 Remove the empty `Infrastructure/` folder
- [x] 11.8 Verify no remaining references to `MongoDB.Driver` types in application code (models, repos, handlers, consumers, controllers)

## 12. Testing & Verification

- [x] 12.1 Verify the project compiles with zero errors
- [x] 12.2 Update existing unit/integration tests in `EShop.Catalog.Tests` to use EF Core `CatalogReadDbContext` instead of `IMongoRepositoryBase<>` mocks
- [x] 12.3 Add integration test: Category projection via consumer → verify data in MongoDB via EF Core
- [x] 12.4 Add integration test: JADNC GET categories returns only current tenant's data (multi-tenancy)
- [x] 12.5 Add integration test: JADNC filtering/sorting/pagination works with EF Core MongoDB provider
- [x] 12.6 Add unit test: `IdempotentConsumer` skips duplicate messages
- [x] 12.7 Verify existing Docker Compose / AppHost configuration still works with the read side service
