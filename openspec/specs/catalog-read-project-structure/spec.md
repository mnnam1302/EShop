## ADDED Requirements

### Requirement: Layered folder structure for read side project

The `EShop.Catalog.ReadModels.MongoDb` project SHALL be organized into the following top-level folders, each with a clear responsibility:
- `Models/` — Read model entities, repository interfaces, and shared model types
- `Persistence/` — EF Core DbContext, repository implementations, and entity configurations
- `Controllers/` — JsonApiDotNetCore controllers
- `Consumers/` — MassTransit integration event consumers
- `Handlers/` — CQRS projection command handlers
- `Bootstrapping/` — DI registration and service configuration

#### Scenario: New read model follows established folder convention
- **WHEN** a developer adds a new read model entity (e.g., `Product`)
- **THEN** the entity class SHALL be placed in `Models/`, its repository interface in `Models/`, its entity configuration in `Persistence/EntityConfigurations/`, its repository implementation in `Persistence/`, its consumer(s) in `Consumers/`, its handler(s) in `Handlers/`, and its controller in `Controllers/`

#### Scenario: No cross-layer direct dependencies
- **WHEN** code in `Models/` is examined
- **THEN** it SHALL NOT reference types from `Persistence/` — dependency inversion dictates that `Models/` defines interfaces and `Persistence/` provides implementations

### Requirement: Models folder contains entities and repository interfaces

The `Models/` folder SHALL contain:
- Read model entity classes decorated with JADNC `[Resource]` and `[Attr]` annotations (e.g., `Category`)
- Repository interfaces (e.g., `ICategoryReadRepository`) that extend `IRepositoryBase<TEntity, TKey>` from the shared library
- Any shared model types or constants used across layers

#### Scenario: Repository interface placed alongside its entity
- **WHEN** `ICategoryReadRepository` is defined
- **THEN** it SHALL reside in the `Models/` folder, co-located with the `Category` entity it operates on

#### Scenario: Entity does not depend on persistence infrastructure
- **WHEN** the `Category` entity class is examined
- **THEN** it SHALL NOT reference EF Core types (e.g., `DbContext`, `DbSet<T>`) — it SHALL only use JADNC annotations and shared domain interfaces (`IEntityBase<T>`, `IScoped`)

### Requirement: Persistence folder contains DbContext and repository implementations

The `Persistence/` folder SHALL contain:
- `CatalogReadDbContext` — the EF Core DbContext for MongoDB
- Repository implementation classes (e.g., `CategoryReadRepository`) extending `RepositoryBase<CatalogReadDbContext, TEntity, TKey>`
- `EntityConfigurations/` subfolder with `IEntityTypeConfiguration<T>` implementations

#### Scenario: DbContext placed in Persistence folder
- **WHEN** the project structure is examined
- **THEN** `CatalogReadDbContext.cs` SHALL reside in `Persistence/`

#### Scenario: Entity configurations in dedicated subfolder
- **WHEN** entity configuration files are examined
- **THEN** `CategoryEntityConfiguration.cs` and `InboxMessageEntityConfiguration.cs` SHALL reside in `Persistence/EntityConfigurations/`

### Requirement: Controllers folder contains JADNC controllers only

The `Controllers/` folder SHALL contain only JsonApiDotNetCore controller classes. Controllers SHALL remain thin wrappers — they SHALL NOT contain business logic, data access, or projection logic.

#### Scenario: CategoriesController is a thin JADNC wrapper
- **WHEN** `CategoriesController` is examined
- **THEN** it SHALL inherit from the appropriate JADNC controller base class, apply authorization attributes (`[RequireFeature]`, `[RequireOneOfPermissions]`), and delegate all query logic to JADNC's built-in repository pipeline

### Requirement: Consumers folder contains MassTransit event consumers

The `Consumers/` folder SHALL contain all MassTransit integration event consumers (e.g., `CategoryCreatedConsumer`, `CategoryUpdatedConsumer`) and the `IdempotentConsumer<T>` base class. Consumers SHALL dispatch to handlers via the mediator — they SHALL NOT directly access the DbContext or repositories.

#### Scenario: Consumer dispatches to handler via mediator
- **WHEN** `CategoryCreatedConsumer` processes a `CategoryCreated` event
- **THEN** it SHALL create a projection command and dispatch it via `IMediator.SendAsync()`, not directly interact with the repository or DbContext

#### Scenario: IdempotentConsumer base class in Consumers folder
- **WHEN** the project structure is examined
- **THEN** `IdempotentConsumer.cs` SHALL reside in `Consumers/`

### Requirement: Handlers folder contains projection command handlers

The `Handlers/` folder SHALL contain CQRS command handlers that process projection commands dispatched by consumers. Handlers SHALL depend on repository interfaces (from `Models/`) for data access.

#### Scenario: Handler uses repository interface for projection
- **WHEN** `CreateCategoryProjectionCommandHandler` processes a command
- **THEN** it SHALL use `ICategoryReadRepository` (injected via DI) to check for existing projections and insert new read model entities

### Requirement: Bootstrapping folder orchestrates all DI registrations

The `Bootstrapping/` folder SHALL contain extension methods that register all services: DbContext, repositories, MassTransit, JADNC, Swagger, and API versioning. The top-level `ServiceCollectionExtensions` SHALL orchestrate registrations by calling focused methods for each concern.

#### Scenario: All DI registration in Bootstrapping
- **WHEN** the application starts and calls `AddBoostrapping()`
- **THEN** all service registrations (DbContext, repositories, MassTransit consumers, JADNC, Swagger) SHALL be performed through methods defined in the `Bootstrapping/` folder

#### Scenario: Each registration concern is a separate method
- **WHEN** `ServiceCollectionExtensions` is examined
- **THEN** it SHALL contain separate methods for each concern: `AddMongoDbPersistence()`, `AddJsonApiDotNet()`, `AddMassTransitRabbitMQ()`, `AddSwagger()`, `AddApiVersioning()`

### Requirement: Package dependencies updated

The project's `.csproj` file SHALL reference `MongoDB.EntityFrameworkCore` and `JsonApiDotNetCore` (EF Core flavor) instead of `JsonApiDotNetCore.MongoDb` and direct `MongoDB.Driver` usage. The `Directory.Packages.props` SHALL include version entries for new packages.

#### Scenario: csproj references EF Core MongoDB provider
- **WHEN** the `EShop.Catalog.ReadModels.MongoDb.csproj` is examined
- **THEN** it SHALL reference `MongoDB.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore` packages

#### Scenario: csproj no longer references MongoDB-specific JADNC
- **WHEN** the `EShop.Catalog.ReadModels.MongoDb.csproj` is examined
- **THEN** it SHALL NOT reference `JsonApiDotNetCore.MongoDb` — it SHALL reference `JsonApiDotNetCore` (the EF Core flavor) instead

#### Scenario: Directory.Packages.props updated with new versions
- **WHEN** the `Directory.Packages.props` is examined
- **THEN** it SHALL contain a `<PackageVersion Include="MongoDB.EntityFrameworkCore" ... />` entry compatible with EF Core 8.x and MongoDB.Driver 3.x
