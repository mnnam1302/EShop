## ADDED Requirements

### Requirement: Product DbSet on CatalogReadDbContext

The `CatalogReadDbContext` SHALL expose a `DbSet<Product>` property for the Product read model entity, alongside the existing `DbSet<Category>` and `DbSet<InboxMessage>`.

#### Scenario: DbContext exposes Product DbSet
- **WHEN** a consumer or handler resolves `CatalogReadDbContext`
- **THEN** the DbContext SHALL expose `DbSet<Product>` for product read model persistence

### Requirement: ProductEntityConfiguration maps Product to MongoDB

The system SHALL provide a `ProductEntityConfiguration` implementing `IEntityTypeConfiguration<Product>` in `Persistence/EntityConfigurations/`. It SHALL map the `Product` entity to the `Product` MongoDB collection, configure `Id` as the key, and configure embedded types for `VariationDimensions`, `Variants`, and nested `DimensionValues`.

#### Scenario: Product entity maps to correct collection
- **WHEN** the `CatalogReadDbContext` model is built
- **THEN** the `Product` entity SHALL be mapped to the MongoDB collection named `Product`

#### Scenario: Product embedded types configured
- **WHEN** `ProductEntityConfiguration` is applied
- **THEN** `VariationDimensions` (list of `ProductVariationDimension`), `Variants` (list of `ProductVariant`), and `Variants[].DimensionValues` (list of `ProductVariantDimensionValue`) SHALL be configured as owned/embedded types

### Requirement: IProductReadRepository and ProductReadRepository

The system SHALL define `IProductReadRepository` extending `IRepositoryBase<Product, string>` in `Models/` and `ProductReadRepository` extending `RepositoryBase<CatalogReadDbContext, Product, string>` in `Persistence/`. The repository SHALL be registered as a scoped DI service.

#### Scenario: ProductReadRepository registered in DI
- **WHEN** the application starts and calls persistence registration
- **THEN** `IProductReadRepository` SHALL be registered as a scoped service resolving to `ProductReadRepository`

#### Scenario: Repository operations go through EF Core
- **WHEN** a repository method (e.g., `FindByIdAsync`) is called on `ProductReadRepository`
- **THEN** the operation SHALL execute through the `CatalogReadDbContext` using EF Core's change tracking and the MongoDB provider

## MODIFIED Requirements

### Requirement: EF Core MongoDB DbContext for read models

The system SHALL provide a `CatalogReadDbContext` that extends `DbContext` and uses the `MongoDB.EntityFrameworkCore` provider to manage all Catalog read model persistence in MongoDB. The `CatalogReadDbContext` SHALL implement `IInboxDbContext` to support idempotent event consumption using the shared `InboxMessage` entity from `EShop.Shared.EventBus`.

#### Scenario: DbContext connects to MongoDB
- **WHEN** the application starts and resolves `CatalogReadDbContext`
- **THEN** the DbContext SHALL be configured with the MongoDB connection string and database name from application settings

#### Scenario: DbContext exposes read model DbSets
- **WHEN** a consumer or handler resolves `CatalogReadDbContext`
- **THEN** the DbContext SHALL expose `DbSet<Category>` for category read models, `DbSet<Product>` for product read models, and `DbSet<InboxMessage>` for idempotency tracking

#### Scenario: Entity configurations applied via IEntityTypeConfiguration
- **WHEN** the DbContext model is built
- **THEN** entity configurations SHALL be applied from `IEntityTypeConfiguration<T>` implementations discovered in the assembly, including `CategoryEntityConfiguration`, `ProductEntityConfiguration`, and `InboxMessageEntityConfiguration`
