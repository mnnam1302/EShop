## ADDED Requirements

### Requirement: Product consumers in Consumers folder

The `Consumers/` folder SHALL contain all 13 Product integration event consumers:
- `ProductCreatedConsumer`, `ProductUpdatedConsumer`, `ProductPublishedConsumer`, `ProductUnpublishedConsumer`, `ProductDeletedConsumer`
- `VariationDimensionAddedConsumer`, `VariationDimensionUpdatedConsumer`, `VariationDimensionValuesChangedConsumer`
- `VariantCreatedConsumer`, `VariantUpdatedConsumer`, `VariantPriceChangedConsumer`, `VariantPublishedConsumer`, `VariantUnpublishedConsumer`

Each consumer SHALL extend `IdempotentConsumer<T>` and dispatch to a projection handler via `IMediator.SendAsync()`.

#### Scenario: Product consumer follows Category consumer pattern
- **WHEN** a new product consumer (e.g., `ProductCreatedConsumer`) is examined
- **THEN** it SHALL follow the same pattern as `CategoryCreatedConsumer`: extend `IdempotentConsumer<T>`, accept `CatalogReadDbContext` and `IMediator` in its constructor, map the integration event to a projection command, and dispatch via mediator

### Requirement: Product projection handlers in Handlers folder

The `Handlers/` folder SHALL contain projection command handlers for all 13 Product events:
- `CreateProductProjectionCommandHandler`, `UpdateProductProjectionCommandHandler`, `PublishProductProjectionCommandHandler`, `UnpublishProductProjectionCommandHandler`, `DeleteProductProjectionCommandHandler`
- `AddVariationDimensionProjectionCommandHandler`, `UpdateVariationDimensionProjectionCommandHandler`, `ChangeVariationDimensionValuesProjectionCommandHandler`
- `CreateVariantProjectionCommandHandler`, `UpdateVariantProjectionCommandHandler`, `ChangeVariantPriceProjectionCommandHandler`, `PublishVariantProjectionCommandHandler`, `UnpublishVariantProjectionCommandHandler`

Each handler SHALL depend on `IProductReadRepository` for data access.

#### Scenario: Handler uses repository interface
- **WHEN** `CreateProductProjectionCommandHandler` processes a command
- **THEN** it SHALL use `IProductReadRepository` (injected via DI) to insert the Product document

### Requirement: Product model files in Models folder

The `Models/` folder SHALL contain the Product read model classes:
- `Product` (main entity with JADNC annotations)
- `ProductVariationDimension` (embedded dimension model)
- `ProductVariant` (embedded variant model)
- `ProductVariantDimensionValue` (embedded dimension value model)
- `IProductReadRepository` (repository interface)

#### Scenario: Product model placed alongside Category model
- **WHEN** the `Models/` folder is examined
- **THEN** `Product.cs`, `ProductVariationDimension.cs`, `ProductVariant.cs`, `ProductVariantDimensionValue.cs`, and `IProductReadRepository.cs` SHALL reside in `Models/`

### Requirement: Product persistence files in Persistence folder

The `Persistence/` folder SHALL contain:
- `ProductReadRepository` in `Persistence/`
- `ProductEntityConfiguration` in `Persistence/EntityConfigurations/`

#### Scenario: ProductEntityConfiguration in EntityConfigurations subfolder
- **WHEN** the `Persistence/EntityConfigurations/` folder is examined
- **THEN** `ProductEntityConfiguration.cs` SHALL reside alongside `CategoryEntityConfiguration.cs` and `InboxMessageEntityConfiguration.cs`

### Requirement: ProductsController in Controllers folder

The `Controllers/` folder SHALL contain a `ProductsController` following the existing `CategoriesController` pattern — a thin JADNC wrapper with authorization attributes.

#### Scenario: ProductsController placed in Controllers folder
- **WHEN** the `Controllers/` folder is examined
- **THEN** `ProductsController.cs` SHALL reside alongside `CategoriesController.cs`

## MODIFIED Requirements

### Requirement: Layered folder structure for read side project

The `EShop.Catalog.ReadModels.MongoDb` project SHALL be organized into the following top-level folders, each with a clear responsibility:
- `Models/` — Read model entities (Category, Product, and embedded types), repository interfaces, and shared model types
- `Persistence/` — EF Core DbContext, repository implementations (Category and Product), and entity configurations
- `Controllers/` — JsonApiDotNetCore controllers (CategoriesController, ProductsController)
- `Consumers/` — MassTransit integration event consumers (Category consumers and 13 Product consumers)
- `Handlers/` — CQRS projection command handlers (Category handlers and 13 Product handlers)
- `Bootstrapping/` — DI registration and service configuration

#### Scenario: New read model follows established folder convention
- **WHEN** a developer adds a new read model entity (e.g., `Product`)
- **THEN** the entity class SHALL be placed in `Models/`, its repository interface in `Models/`, its entity configuration in `Persistence/EntityConfigurations/`, its repository implementation in `Persistence/`, its consumer(s) in `Consumers/`, its handler(s) in `Handlers/`, and its controller in `Controllers/`

#### Scenario: No cross-layer direct dependencies
- **WHEN** code in `Models/` is examined
- **THEN** it SHALL NOT reference types from `Persistence/` — dependency inversion dictates that `Models/` defines interfaces and `Persistence/` provides implementations

### Requirement: Bootstrapping folder orchestrates all DI registrations

The `Bootstrapping/` folder SHALL contain extension methods that register all services: DbContext, repositories (Category and Product), MassTransit (Category and Product consumers), JADNC, Swagger, and API versioning. The top-level `ServiceCollectionExtensions` SHALL orchestrate registrations by calling focused methods for each concern.

#### Scenario: All DI registration in Bootstrapping
- **WHEN** the application starts and calls `AddBoostrapping()`
- **THEN** all service registrations (DbContext, repositories including `IProductReadRepository`, MassTransit consumers including 13 Product consumers, JADNC, Swagger) SHALL be performed through methods defined in the `Bootstrapping/` folder

#### Scenario: Each registration concern is a separate method
- **WHEN** `ServiceCollectionExtensions` is examined
- **THEN** it SHALL contain separate methods for each concern: `AddMongoDbPersistence()`, `AddJsonApiDotNet()`, `AddMassTransitRabbitMQ()`, `AddSwagger()`, `AddApiVersioning()`
