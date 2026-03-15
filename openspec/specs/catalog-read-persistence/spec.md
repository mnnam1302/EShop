## ADDED Requirements

### Requirement: EF Core MongoDB DbContext for read models

The system SHALL provide a `CatalogReadDbContext` that extends `DbContext` and uses the `MongoDB.EntityFrameworkCore` provider to manage all Catalog read model persistence in MongoDB. The `CatalogReadDbContext` SHALL implement `IInboxDbContext` to support idempotent event consumption using the shared `InboxMessage` entity from `EShop.Shared.EventBus`.

#### Scenario: DbContext connects to MongoDB
- **WHEN** the application starts and resolves `CatalogReadDbContext`
- **THEN** the DbContext SHALL be configured with the MongoDB connection string and database name from application settings

#### Scenario: DbContext exposes read model DbSets
- **WHEN** a consumer or handler resolves `CatalogReadDbContext`
- **THEN** the DbContext SHALL expose `DbSet<Category>` for category read models and `DbSet<InboxMessage>` for idempotency tracking

#### Scenario: Entity configurations applied via IEntityTypeConfiguration
- **WHEN** the DbContext model is built
- **THEN** entity configurations SHALL be applied from `IEntityTypeConfiguration<T>` implementations discovered in the assembly, mapping entities to their MongoDB collections

### Requirement: Category entity configuration maps to MongoDB collection

The system SHALL provide a `CategoryEntityConfiguration` implementing `IEntityTypeConfiguration<Category>` that maps the `Category` entity to the `Category` MongoDB collection, configures `Id` as the key, and maps all properties to their corresponding BSON fields.

#### Scenario: Category entity maps to correct collection
- **WHEN** the `CatalogReadDbContext` model is built
- **THEN** the `Category` entity SHALL be mapped to the MongoDB collection named `Category`

#### Scenario: Category key is string Id
- **WHEN** a `Category` entity is persisted
- **THEN** the `Id` property (string) SHALL be stored as the MongoDB `_id` field

### Requirement: InboxMessage entity configuration for idempotency

The system SHALL provide an `InboxMessageEntityConfiguration` implementing `IEntityTypeConfiguration<InboxMessage>` that maps the shared `InboxMessage` entity to the `InboxMessages` MongoDB collection.

#### Scenario: InboxMessage maps to correct collection
- **WHEN** the `CatalogReadDbContext` model is built
- **THEN** the `InboxMessage` entity SHALL be mapped to the MongoDB collection named `InboxMessages`

### Requirement: Repository interfaces defined alongside models

The system SHALL define repository interfaces (e.g., `ICategoryReadRepository`) in the `Models` namespace/folder, following the Dependency Inversion Principle. Handlers and consumers SHALL depend on these interfaces, not on concrete implementations or the `DbContext` directly.

#### Scenario: Handler depends on repository interface
- **WHEN** a projection command handler (e.g., `CreateCategoryProjectionCommandHandler`) needs to persist a read model
- **THEN** it SHALL depend on `ICategoryReadRepository` (injected via DI), not on `CatalogReadDbContext`

#### Scenario: Repository interface extends shared IRepositoryBase
- **WHEN** `ICategoryReadRepository` is defined
- **THEN** it SHALL extend `IRepositoryBase<Category, string>` from `EShop.Shared.DomainTools`

### Requirement: Repository implementations use shared RepositoryBase

The system SHALL provide repository implementations (e.g., `CategoryReadRepository`) in the `Persistence` namespace/folder that extend `RepositoryBase<CatalogReadDbContext, TEntity, TKey>` from `EShop.Shared.DomainTools`. These implementations SHALL be registered in DI as their corresponding interface.

#### Scenario: CategoryReadRepository registered in DI
- **WHEN** the application starts
- **THEN** `ICategoryReadRepository` SHALL be registered as a scoped service resolving to `CategoryReadRepository`

#### Scenario: Repository operations go through EF Core
- **WHEN** a repository method (e.g., `Add`, `FindByIdAsync`) is called
- **THEN** the operation SHALL execute through the `CatalogReadDbContext` using EF Core's change tracking and the MongoDB provider

### Requirement: Read model entities implement IEntityBase and IScoped

All tenant-scoped read model entities (e.g., `Category`) SHALL implement `IEntityBase<string>` from `EShop.Shared.DomainTools` with `Id` as the MongoDB `_id` (string type), and `IScoped` with `TenantId` and `Scope` properties for multi-tenancy filtering.

#### Scenario: Category implements required interfaces
- **WHEN** the `Category` read model entity is defined
- **THEN** it SHALL implement `IEntityBase<string>` and `IScoped`

#### Scenario: Category retains JADNC resource annotations
- **WHEN** the `Category` entity is defined
- **THEN** it SHALL retain `[Resource]` and `[Attr]` annotations for JsonApiDotNetCore compatibility

#### Scenario: DocumentId preserved for aggregate linkage
- **WHEN** a `Category` entity is persisted
- **THEN** it SHALL have a `DocumentId` (Guid) property that stores the write-side aggregate ID, separate from the MongoDB `Id` (string)

### Requirement: Idempotent consumer uses EF Core IInboxDbContext

The `IdempotentConsumer<T>` base class SHALL use `CatalogReadDbContext` (via `IInboxDbContext`) instead of `IMongoRepositoryBase<InboxMessage>` for idempotency checks and inbox message persistence. The idempotency check SHALL query `DbSet<InboxMessage>` by `MessageId` and `ConsumerId`.

#### Scenario: Duplicate message detected and skipped
- **WHEN** a MassTransit consumer receives a message whose `MessageId` + `ConsumerId` combination already exists in the `InboxMessages` collection
- **THEN** the consumer SHALL return without processing the message

#### Scenario: New message processed and inbox updated
- **WHEN** a MassTransit consumer receives a message not yet in the inbox
- **THEN** the consumer SHALL process the message, create an `InboxMessage` entity, mark it as Done or Failed, and persist it via `DbContext.SaveChangesAsync()`

### Requirement: Remove raw MongoDB driver abstractions

The system SHALL remove all custom MongoDB driver abstractions: `IDocument`, `Document`, `IMongoRepositoryBase<T>`, `MongoRepositoryBase<T>`, `MongoCollectionAttribute`, and `IMongoDbSettings`. These SHALL be fully replaced by EF Core DbContext, entity configurations, and repository pattern implementations.

#### Scenario: No direct MongoDB.Driver usage in application code
- **WHEN** the refactoring is complete
- **THEN** no application code (models, repositories, handlers, consumers, controllers) SHALL reference `MongoDB.Driver` types directly — all data access SHALL go through EF Core's `DbContext`

### Requirement: JADNC uses EF Core integration

The system SHALL replace `JsonApiDotNetCore.MongoDb` with `JsonApiDotNetCore` (EF Core integration). JADNC SHALL be configured to use `EntityFrameworkCoreRepository<TResource, TId>` as the default resource repository, querying resources through the `CatalogReadDbContext`.

#### Scenario: JADNC queries go through DbContext
- **WHEN** a JSON:API GET request is received for categories
- **THEN** JADNC SHALL execute the query through `CatalogReadDbContext.Set<Category>()`, which applies global query filters (including tenant scoping)

#### Scenario: JADNC resource registration uses EF Core
- **WHEN** the application starts and configures JADNC
- **THEN** JADNC SHALL register resources via `AddJsonApi()` and use `AddEntityFrameworkCoreRepository<CatalogReadDbContext>()` instead of `AddJsonApiMongoDb()`
