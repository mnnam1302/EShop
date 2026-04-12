## ADDED Requirements

### Requirement: Product read model document structure

The system SHALL define a `Product` read model entity in the `Models/` folder implementing `IEntityBase<string>` and `IScoped`. The entity SHALL have the following properties:
- `Id` (string) — MongoDB `_id`, the string representation of the aggregate ID
- `DocumentId` (Guid) — write-side aggregate ID for linkage
- `Version` (ulong) — event version for optimistic concurrency
- `Name` (string), `Description` (string), `Slug` (string)
- `CategoryId` (string) — string representation of the category aggregate ID
- `Tags` (string[]), `Images` (string[])
- `State` (string) — Product state name ("Draft", "Published", "Unpublished", "Deleted")
- `VariationDimensions` (list of `ProductVariationDimension`)
- `Variants` (list of `ProductVariant`)
- `TenantId` (string), `Scope` (string)
- `CreatedByUserId` (string), `CreatedAtUtc` (DateTimeOffset)
- `LastModifiedByUserId` (string?), `LastModifiedAtUtc` (DateTimeOffset?)

The entity SHALL be decorated with JADNC `[Resource]` and `[Attr]` annotations.

#### Scenario: Product entity implements required interfaces
- **WHEN** the `Product` read model entity is defined
- **THEN** it SHALL implement `IEntityBase<string>` and `IScoped`

#### Scenario: Product entity has JADNC annotations
- **WHEN** the `Product` entity is examined
- **THEN** all public properties exposed to the API SHALL have `[Attr]` annotations

### Requirement: ProductVariationDimension embedded model

The system SHALL define a `ProductVariationDimension` class in `Models/` with properties:
- `Name` (string), `DisplayName` (string), `Values` (string[]), `DisplayStyle` (string)

This class SHALL be embedded in the Product document as an array, not a separate collection.

#### Scenario: Dimension embedded in product document
- **WHEN** a Product with 2 variation dimensions is persisted to MongoDB
- **THEN** the `variationDimensions` field SHALL be an array of 2 embedded subdocuments within the Product document

### Requirement: ProductVariant embedded model

The system SHALL define a `ProductVariant` class in `Models/` with properties:
- `Id` (string), `Name` (string), `Sku` (string)
- `Price` (decimal), `DiscountPrice` (decimal)
- `IsDefault` (bool), `State` (string)
- `DimensionValues` (list of `ProductVariantDimensionValue`)

This class SHALL be embedded in the Product document as an array.

#### Scenario: Variant embedded in product document
- **WHEN** a Product with 3 variants is persisted to MongoDB
- **THEN** the `variants` field SHALL be an array of 3 embedded subdocuments within the Product document

### Requirement: ProductVariantDimensionValue embedded model

The system SHALL define a `ProductVariantDimensionValue` class in `Models/` with properties:
- `Name` (string), `Value` (string)

#### Scenario: Dimension values embedded in variant
- **WHEN** a variant has 2 dimension values (Color=Red, Size=M)
- **THEN** the `dimensionValues` field within the variant subdocument SHALL contain 2 entries

### Requirement: Product repository interface

The system SHALL define an `IProductReadRepository` interface in `Models/` extending `IRepositoryBase<Product, string>`. Projection handlers SHALL depend on this interface for data access.

#### Scenario: Repository interface extends shared base
- **WHEN** `IProductReadRepository` is defined
- **THEN** it SHALL extend `IRepositoryBase<Product, string>` from `EShop.Shared.DomainTools`

### Requirement: Product repository implementation

The system SHALL provide a `ProductReadRepository` class in `Persistence/` extending `RepositoryBase<CatalogReadDbContext, Product, string>`. It SHALL be registered in DI as `IProductReadRepository` (scoped lifetime).

#### Scenario: ProductReadRepository registered in DI
- **WHEN** the application starts
- **THEN** `IProductReadRepository` SHALL be registered as a scoped service resolving to `ProductReadRepository`

### Requirement: Product entity configuration for MongoDB

The system SHALL provide a `ProductEntityConfiguration` implementing `IEntityTypeConfiguration<Product>` that maps the `Product` entity to the `Product` MongoDB collection, configures `Id` as the key, and maps embedded types (`VariationDimensions`, `Variants`, and their nested `DimensionValues`).

#### Scenario: Product maps to correct collection
- **WHEN** the `CatalogReadDbContext` model is built
- **THEN** the `Product` entity SHALL be mapped to the MongoDB collection named `Product`

#### Scenario: Embedded types configured
- **WHEN** the `ProductEntityConfiguration` is applied
- **THEN** `VariationDimensions`, `Variants`, and nested `DimensionValues` SHALL be configured as owned types or embedded documents

### Requirement: ProductCreated consumer and projection handler

A `ProductCreatedConsumer` extending `IdempotentConsumer<ProductCreated>` SHALL be created in `Consumers/`. It SHALL map the integration event to a `CreateProductProjectionCommand` and dispatch via `IMediator.SendAsync()`.

A `CreateProductProjectionCommandHandler` SHALL be created in `Handlers/`. It SHALL use `IProductReadRepository` to insert a new `Product` document with all fields from the integration event, initial state "Draft", empty VariationDimensions and Variants arrays.

#### Scenario: New product document created on ProductCreated event
- **WHEN** a `ProductCreated` integration event is received
- **THEN** a new Product document SHALL be inserted into MongoDB with state "Draft", version 1, and all product metadata

#### Scenario: Duplicate ProductCreated event is idempotent
- **WHEN** a `ProductCreated` event with the same MessageId is received twice
- **THEN** the second processing SHALL be skipped via inbox deduplication

### Requirement: ProductUpdated consumer and projection handler

A `ProductUpdatedConsumer` extending `IdempotentConsumer<ProductUpdated>` SHALL be created. A `UpdateProductProjectionCommandHandler` SHALL update the existing Product document's Name, Description, CategoryId, Tags, Slug, Images, Groups, and LastModified fields.

#### Scenario: Product document updated on ProductUpdated event
- **WHEN** a `ProductUpdated` integration event is received
- **THEN** the existing Product document SHALL be updated with the new metadata and LastModifiedAtUtc/LastModifiedByUserId

### Requirement: ProductPublished consumer and projection handler

A `ProductPublishedConsumer` SHALL be created. The projection handler SHALL update the Product document's State to "Published" and update LastModified fields.

#### Scenario: Product state changes to Published
- **WHEN** a `ProductPublished` integration event is received
- **THEN** the Product document's State field SHALL become "Published"

### Requirement: ProductUnpublished consumer and projection handler

A `ProductUnpublishedConsumer` SHALL be created. The projection handler SHALL update the Product document's State to "Unpublished".

#### Scenario: Product state changes to Unpublished
- **WHEN** a `ProductUnpublished` integration event is received
- **THEN** the Product document's State field SHALL become "Unpublished"

### Requirement: ProductDeleted consumer and projection handler

A `ProductDeletedConsumer` SHALL be created. The projection handler SHALL update the Product document's State to "Deleted".

#### Scenario: Product state changes to Deleted
- **WHEN** a `ProductDeleted` integration event is received
- **THEN** the Product document's State field SHALL become "Deleted"

### Requirement: VariationDimensionAdded consumer and projection handler

A `VariationDimensionAddedConsumer` SHALL be created. The projection handler SHALL push a new `ProductVariationDimension` subdocument into the Product document's `VariationDimensions` array.

#### Scenario: Dimension added to product document
- **WHEN** a `VariationDimensionAdded` integration event is received
- **THEN** a new dimension subdocument SHALL be appended to the Product document's VariationDimensions array

### Requirement: VariationDimensionUpdated consumer and projection handler

A `VariationDimensionUpdatedConsumer` SHALL be created. The projection handler SHALL find the matching dimension by Name in the VariationDimensions array and update its DisplayName and DisplayStyle.

#### Scenario: Dimension metadata updated in product document
- **WHEN** a `VariationDimensionUpdated` integration event is received with Name="Color", DisplayName="Colour"
- **THEN** the matching dimension subdocument's DisplayName SHALL become "Colour"

### Requirement: VariationDimensionValuesChanged consumer and projection handler

A `VariationDimensionValuesChangedConsumer` SHALL be created. The projection handler SHALL find the matching dimension by Name and replace its Values array.

#### Scenario: Dimension values replaced in product document
- **WHEN** a `VariationDimensionValuesChanged` integration event is received with DimensionName="Color", Values=["Red", "Blue", "Green"]
- **THEN** the matching dimension's Values SHALL become ["Red", "Blue", "Green"]

### Requirement: VariantCreated consumer and projection handler

A `VariantCreatedConsumer` SHALL be created. The projection handler SHALL push a new `ProductVariant` subdocument into the Product document's Variants array with state "Unpublished".

#### Scenario: Variant added to product document
- **WHEN** a `VariantCreated` integration event is received
- **THEN** a new variant subdocument SHALL be appended to the Product document's Variants array with State "Unpublished"

### Requirement: VariantUpdated consumer and projection handler

A `VariantUpdatedConsumer` SHALL be created. The projection handler SHALL find the matching variant by ID in the Variants array and update its Name and Sku.

#### Scenario: Variant metadata updated in product document
- **WHEN** a `VariantUpdated` integration event is received with VariantId and new Name/Sku
- **THEN** the matching variant subdocument's Name and Sku SHALL be updated

### Requirement: VariantPriceChanged consumer and projection handler

A `VariantPriceChangedConsumer` SHALL be created. The projection handler SHALL find the matching variant by ID and update its Price and DiscountPrice to the new values.

#### Scenario: Variant price updated in product document
- **WHEN** a `VariantPriceChanged` integration event is received with NewPrice=39.99, NewDiscountPrice=34.99
- **THEN** the matching variant's Price SHALL become 39.99 and DiscountPrice SHALL become 34.99

### Requirement: VariantPublished consumer and projection handler

A `VariantPublishedConsumer` SHALL be created. The projection handler SHALL find the matching variant by ID and update its State to "Published".

#### Scenario: Variant state changes to Published
- **WHEN** a `VariantPublished` integration event is received
- **THEN** the matching variant subdocument's State SHALL become "Published"

### Requirement: VariantUnpublished consumer and projection handler

A `VariantUnpublishedConsumer` SHALL be created. The projection handler SHALL find the matching variant by ID and update its State to "Unpublished".

#### Scenario: Variant state changes to Unpublished
- **WHEN** a `VariantUnpublished` integration event is received
- **THEN** the matching variant subdocument's State SHALL become "Unpublished"

### Requirement: MongoDB indexes for Product collection

The `ProductEntityConfiguration` SHALL configure the following indexes on the Product collection:
1. Compound index on `{ TenantId, State }` for product listing queries
2. Compound index on `{ TenantId, CategoryId }` for category-filtered queries
3. Compound index on `{ TenantId, Slug }` (unique) for URL-based product routing
4. Compound index on `{ TenantId, "Variants.Sku" }` for SKU lookup
5. Compound index on `{ TenantId, "Variants.Price" }` for price range queries

#### Scenario: Tenant + State index enables listing queries
- **WHEN** a query filters Products by TenantId and State = "Published"
- **THEN** the query SHALL use the `{ TenantId, State }` index for efficient execution

#### Scenario: Tenant + Slug index enforces uniqueness per tenant
- **WHEN** two Products in the same tenant have the same Slug
- **THEN** the unique index SHALL prevent the duplicate from being inserted

#### Scenario: Variant SKU index enables lookup
- **WHEN** a query searches for a variant by SKU within a tenant
- **THEN** the query SHALL use the `{ TenantId, "Variants.Sku" }` index

### Requirement: Product JADNC controller

A `ProductsController` SHALL be created in `Controllers/` inheriting from the appropriate JADNC controller base class. It SHALL apply `[RequireFeature]` and `[RequireOneOfPermissions]` attributes following the existing `CategoriesController` pattern.

#### Scenario: ProductsController serves JSON:API GET requests
- **WHEN** a GET request is sent to the Products JSON:API endpoint
- **THEN** JADNC SHALL query through `CatalogReadDbContext.Set<Product>()` with tenant-scoped global query filters applied

#### Scenario: ProductsController is a thin wrapper
- **WHEN** `ProductsController` is examined
- **THEN** it SHALL NOT contain business logic, data access code, or projection logic — only JADNC controller base class delegation and authorization attributes

### Requirement: MassTransit consumer registration

All 13 Product consumers SHALL be registered with MassTransit in the `AddMassTransitRabbitMQ()` bootstrapping method, following the existing pattern for Category consumers.

#### Scenario: All consumers discovered and registered
- **WHEN** the application starts and MassTransit is configured
- **THEN** all 13 Product consumers SHALL be registered and listening on their respective queues
