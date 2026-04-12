## ADDED Requirements

### Requirement: Rename VariantAddedEvent to VariantCreatedEvent

The existing `VariantAddedEvent` SHALL be renamed to `VariantCreatedEvent` for naming consistency across all domain events. The `[EventVersion]` attribute SHALL be updated accordingly. The `AddVariant()` behavior method and its Apply handler SHALL reference the renamed event.

#### Scenario: Existing AddVariant behavior emits VariantCreatedEvent
- **WHEN** `AddVariant()` is called on the ProductAggregate
- **THEN** a `VariantCreatedEvent` SHALL be raised (not `VariantAddedEvent`)

#### Scenario: Apply handler processes VariantCreatedEvent
- **WHEN** a `VariantCreatedEvent` is replayed during aggregate hydration
- **THEN** the Apply handler SHALL add the variant to the Variants list with the correct state (`Unpublished`)

### Requirement: Variant price fields use decimal type

The `Variant` entity SHALL use `decimal` for `Price` and `DiscountPrice` properties instead of `double`. The `VariantCreatedEvent`, `VariantPriceChangedEvent`, and all commands that carry price data SHALL also use `decimal`.

#### Scenario: Variant created with decimal price
- **WHEN** a variant is created with Price = 29.99m and DiscountPrice = 0m
- **THEN** the Variant entity SHALL store these as `decimal` values without floating-point precision loss

#### Scenario: Price comparison uses decimal semantics
- **WHEN** a specification checks `Price > 0`
- **THEN** the comparison SHALL use decimal arithmetic (not double)

### Requirement: ProductCanAddVariantSpecification enhanced with value membership and combination uniqueness

The existing `ProductCanAddVariantSpecification` SHALL be enhanced to additionally validate:
1. Each dimension value's `Value` is a member of the corresponding `VariationDimension.Values` array
2. No existing non-deleted variant has the same combination of dimension values

#### Scenario: Dimension value not in allowed values rejected
- **WHEN** a variant is being added with DimensionValue ("Color", "Purple") but the Color dimension only has values ["Red", "Blue"]
- **THEN** a domain error SHALL be thrown indicating "Purple" is not a valid value for dimension "Color"

#### Scenario: Duplicate variant combination rejected
- **WHEN** a variant with DimensionValues [("Color", "Red"), ("Size", "M")] already exists and another variant with the same combination is added
- **THEN** a domain error SHALL be thrown indicating a variant with this dimension value combination already exists

#### Scenario: Deleted variant combination does not block new variant
- **WHEN** a variant with DimensionValues [("Color", "Red"), ("Size", "M")] exists but is in Deleted state, and a new variant with the same combination is added
- **THEN** the specification SHALL be satisfied

### Requirement: Update variant metadata

The `ProductAggregate` SHALL expose an `UpdateVariant()` behavior method that validates the `CanUpdateVariantSpecification` and raises a `VariantUpdatedEvent`. The Apply handler SHALL update the matching variant's `Name` and `Sku` in the Variants list.

#### Scenario: Update variant name and SKU
- **WHEN** `UpdateVariant(variantId, "Red / Large", "SHIRT-RED-L")` is called for an existing non-deleted variant
- **THEN** a `VariantUpdatedEvent` SHALL be raised and the variant's Name and Sku SHALL be updated

#### Scenario: Update fails when variant does not exist
- **WHEN** `UpdateVariant()` is called with a variantId that does not exist in the Variants list
- **THEN** a domain error SHALL be thrown indicating the variant was not found

#### Scenario: Update fails when variant is Deleted
- **WHEN** `UpdateVariant()` is called for a variant in Deleted state
- **THEN** a domain error SHALL be thrown indicating a deleted variant cannot be updated

#### Scenario: Update fails when product is Deleted
- **WHEN** the Product is in Deleted state and `UpdateVariant()` is called
- **THEN** a domain error SHALL be thrown

### Requirement: CanUpdateVariantSpecification enforces update rules

The `CanUpdateVariantSpecification` SHALL validate that:
1. The Product is not in Deleted state
2. A variant with the specified ID exists in the Variants list
3. The variant is not in Deleted state

#### Scenario: Valid variant passes
- **WHEN** the Product is in Published state and the variant is in Published state
- **THEN** the specification SHALL be satisfied

#### Scenario: Non-existent variant fails
- **WHEN** the specified variant ID is not found
- **THEN** the specification SHALL report that the variant does not exist

### Requirement: VariantUpdatedEvent domain event

The `VariantUpdatedEvent` SHALL contain `ProductId` (Guid), `VariantId` (Guid), `Name` (string), `Sku` (string), `UpdatedAtUtc` (DateTimeOffset), and `UpdatedByUserId` (string).

#### Scenario: Event carries updated metadata
- **WHEN** a `VariantUpdatedEvent` is raised
- **THEN** it SHALL contain the variant ID, new name, new SKU, timestamp, and user ID

### Requirement: Change variant price

The `ProductAggregate` SHALL expose a `ChangeVariantPrice()` behavior method that validates the `CanChangeVariantPriceSpecification` and raises a `VariantPriceChangedEvent`. The Apply handler SHALL update the matching variant's `Price` and `DiscountPrice`.

#### Scenario: Change price of a variant
- **WHEN** `ChangeVariantPrice(variantId, 39.99m, 34.99m)` is called for an existing non-deleted variant
- **THEN** a `VariantPriceChangedEvent` SHALL be raised and the variant's Price SHALL become 39.99 and DiscountPrice SHALL become 34.99

#### Scenario: Change price fails when price is zero
- **WHEN** `ChangeVariantPrice(variantId, 0m, 0m)` is called
- **THEN** a domain error SHALL be thrown indicating Price must be greater than zero

#### Scenario: Change price fails when discount exceeds price
- **WHEN** `ChangeVariantPrice(variantId, 29.99m, 39.99m)` is called where DiscountPrice > Price
- **THEN** a domain error SHALL be thrown indicating DiscountPrice must be less than or equal to Price

#### Scenario: Change price fails when variant is Deleted
- **WHEN** `ChangeVariantPrice()` is called for a variant in Deleted state
- **THEN** a domain error SHALL be thrown

### Requirement: CanChangeVariantPriceSpecification enforces price rules

The `CanChangeVariantPriceSpecification` SHALL validate that:
1. The Product is not in Deleted state
2. A variant with the specified ID exists and is not Deleted
3. Price is greater than zero
4. DiscountPrice is greater than or equal to zero
5. DiscountPrice is less than or equal to Price

#### Scenario: Valid price change passes
- **WHEN** Price = 29.99m, DiscountPrice = 24.99m, variant exists and is not Deleted
- **THEN** the specification SHALL be satisfied

#### Scenario: Negative discount price fails
- **WHEN** DiscountPrice = -5.00m
- **THEN** the specification SHALL report that DiscountPrice must be greater than or equal to zero

#### Scenario: Zero discount price passes (no discount)
- **WHEN** Price = 29.99m, DiscountPrice = 0m
- **THEN** the specification SHALL be satisfied

### Requirement: VariantPriceChangedEvent domain event

The `VariantPriceChangedEvent` SHALL contain `ProductId` (Guid), `VariantId` (Guid), `OldPrice` (decimal), `NewPrice` (decimal), `OldDiscountPrice` (decimal), `NewDiscountPrice` (decimal), `ChangedAtUtc` (DateTimeOffset), and `ChangedByUserId` (string).

#### Scenario: Event carries old and new prices
- **WHEN** a `VariantPriceChangedEvent` is raised
- **THEN** it SHALL contain both old and new values for Price and DiscountPrice for audit trail purposes

### Requirement: Publish variant

The `ProductAggregate` SHALL expose a `PublishVariant()` behavior method that validates the `CanPublishVariantSpecification` and raises a `VariantPublishedEvent`. The Apply handler SHALL set the matching variant's `State` to `VariantState.Published`.

#### Scenario: Publish an Unpublished variant
- **WHEN** the variant is in Unpublished state, has a non-empty SKU, Price > 0, and all dimension values present, and `PublishVariant(variantId)` is called
- **THEN** a `VariantPublishedEvent` SHALL be raised and the variant's State SHALL become Published

#### Scenario: Publish fails when SKU is empty
- **WHEN** the variant has an empty SKU and `PublishVariant()` is called
- **THEN** a domain error SHALL be thrown indicating SKU is required

#### Scenario: Publish fails when price is zero
- **WHEN** the variant has Price = 0 and `PublishVariant()` is called
- **THEN** a domain error SHALL be thrown indicating Price must be greater than zero

#### Scenario: Publish fails when dimension values are incomplete
- **WHEN** the Product has 2 variation dimensions but the variant has only 1 dimension value (non-default variant)
- **THEN** a domain error SHALL be thrown indicating all dimension values must be present

#### Scenario: Publish default variant without dimension values succeeds
- **WHEN** the variant is the default variant (IsDefault = true) with no dimension values, has SKU and Price > 0
- **THEN** the specification SHALL be satisfied and the variant SHALL be published

#### Scenario: Publish fails when variant is already Published
- **WHEN** the variant is already in Published state
- **THEN** a domain error SHALL be thrown indicating the variant is already published

### Requirement: CanPublishVariantSpecification enforces publish rules

The `CanPublishVariantSpecification` SHALL validate that:
1. The Product is not in Deleted state
2. A variant with the specified ID exists and is not Deleted
3. The variant is in Unpublished state
4. SKU is not null or empty
5. Price is greater than zero
6. For non-default variants: the number of dimension values matches the product's VariationDimensions count and each dimension is covered

#### Scenario: All prerequisites met for non-default variant
- **WHEN** variant is Unpublished, SKU = "SHIRT-RED-M", Price = 29.99m, and DimensionValues cover all product dimensions
- **THEN** the specification SHALL be satisfied

### Requirement: VariantPublishedEvent domain event

The `VariantPublishedEvent` SHALL contain `ProductId` (Guid), `VariantId` (Guid), `PublishedAtUtc` (DateTimeOffset), and `PublishedByUserId` (string).

#### Scenario: Event raised on variant publish
- **WHEN** `PublishVariant()` is called and the specification is satisfied
- **THEN** a `VariantPublishedEvent` SHALL be raised with the correct IDs, timestamp, and user ID

### Requirement: Unpublish variant

The `ProductAggregate` SHALL expose an `UnpublishVariant()` behavior method that validates the `CanUnpublishVariantSpecification` and raises a `VariantUnpublishedEvent`. The Apply handler SHALL set the matching variant's `State` to `VariantState.Unpublished`.

#### Scenario: Unpublish a Published variant when others remain published
- **WHEN** the Product has 2 Published variants and `UnpublishVariant(variantId)` is called for one
- **THEN** a `VariantUnpublishedEvent` SHALL be raised and the variant's State SHALL become Unpublished

#### Scenario: Unpublish fails when it is the last published variant and Product is Published
- **WHEN** the Product is in Published state, has exactly 1 Published variant, and `UnpublishVariant()` is called for that variant
- **THEN** a domain error SHALL be thrown indicating the last published variant cannot be unpublished while the Product is Published

#### Scenario: Unpublish last variant succeeds when Product is not Published
- **WHEN** the Product is in Draft or Unpublished state, has 1 Published variant, and `UnpublishVariant()` is called
- **THEN** a `VariantUnpublishedEvent` SHALL be raised (the guard only applies when the Product itself is Published)

#### Scenario: Unpublish fails when variant is not Published
- **WHEN** the variant is in Unpublished state and `UnpublishVariant()` is called
- **THEN** a domain error SHALL be thrown indicating the variant is not in Published state

### Requirement: CanUnpublishVariantSpecification enforces unpublish rules

The `CanUnpublishVariantSpecification` SHALL validate that:
1. The Product is not in Deleted state
2. A variant with the specified ID exists and is not Deleted
3. The variant is in Published state
4. The variant is NOT the last published variant if the Product is in Published state

#### Scenario: Second-to-last published variant passes when Product is Published
- **WHEN** the Product is Published and has 3 Published variants, unpublishing one
- **THEN** the specification SHALL be satisfied (2 remain published)

#### Scenario: Last published variant blocked when Product is Published
- **WHEN** the Product is Published and has 1 Published variant
- **THEN** the specification SHALL report that the last published variant cannot be unpublished while the product is published

### Requirement: VariantUnpublishedEvent domain event

The `VariantUnpublishedEvent` SHALL contain `ProductId` (Guid), `VariantId` (Guid), `UnpublishedAtUtc` (DateTimeOffset), and `UnpublishedByUserId` (string).

#### Scenario: Event raised on variant unpublish
- **WHEN** `UnpublishVariant()` is called and the specification is satisfied
- **THEN** a `VariantUnpublishedEvent` SHALL be raised with the correct IDs, timestamp, and user ID

### Requirement: UpdateVariant vertical slice

An `UpdateVariant/` folder SHALL be created under `Products/` containing `UpdateVariantCommand`, `UpdateVariantCommandHandler`, `VariantUpdatedEvent`, `CanUpdateVariantSpecification`, and `UpdateVariantEndpointHandler`.

#### Scenario: Endpoint accepts PUT with variant metadata
- **WHEN** a PUT request is sent to the update-variant endpoint with product ID, variant ID, new name, and new SKU
- **THEN** the endpoint handler SHALL create an `UpdateVariantCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: ChangeVariantPrice vertical slice

A `ChangeVariantPrice/` folder SHALL be created under `Products/` containing `ChangeVariantPriceCommand`, `ChangeVariantPriceCommandHandler`, `VariantPriceChangedEvent`, `CanChangeVariantPriceSpecification`, and `ChangeVariantPriceEndpointHandler`.

#### Scenario: Endpoint accepts PUT with price data
- **WHEN** a PUT request is sent to the change-variant-price endpoint with product ID, variant ID, price, and discount price
- **THEN** the endpoint handler SHALL create a `ChangeVariantPriceCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: PublishVariant vertical slice

A `PublishVariant/` folder SHALL be created under `Products/` containing `PublishVariantCommand`, `PublishVariantCommandHandler`, `VariantPublishedEvent`, `CanPublishVariantSpecification`, and `PublishVariantEndpointHandler`.

#### Scenario: Endpoint accepts POST to publish a variant
- **WHEN** a POST request is sent to the publish-variant endpoint with product ID and variant ID
- **THEN** the endpoint handler SHALL create a `PublishVariantCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: UnpublishVariant vertical slice

An `UnpublishVariant/` folder SHALL be created under `Products/` containing `UnpublishVariantCommand`, `UnpublishVariantCommandHandler`, `VariantUnpublishedEvent`, `CanUnpublishVariantSpecification`, and `UnpublishVariantEndpointHandler`.

#### Scenario: Endpoint accepts POST to unpublish a variant
- **WHEN** a POST request is sent to the unpublish-variant endpoint with product ID and variant ID
- **THEN** the endpoint handler SHALL create an `UnpublishVariantCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: Integration events for variant operations

Five new integration event classes SHALL be added to `ProductIntegrationEvents` in `EShop.Shared.Contracts`:
- `VariantCreated` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `VariantId` (Guid), `Name` (string), `Sku` (string), `Price` (decimal), `DiscountPrice` (decimal), `IsDefault` (bool), `VariantDimensionValues` (list of name/value pairs)
- `VariantUpdated` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `VariantId` (Guid), `Name` (string), `Sku` (string)
- `VariantPriceChanged` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `VariantId` (Guid), `OldPrice` (decimal), `NewPrice` (decimal), `OldDiscountPrice` (decimal), `NewDiscountPrice` (decimal)
- `VariantPublished` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `VariantId` (Guid)
- `VariantUnpublished` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `VariantId` (Guid)

Each domain event subscriber SHALL publish the corresponding integration event via `IEventBus`.

#### Scenario: VariantCreated integration event published
- **WHEN** a `VariantCreatedEvent` domain event is raised
- **THEN** a domain event subscriber SHALL publish a `VariantCreated` integration event to RabbitMQ with all variant data

#### Scenario: VariantPriceChanged integration event published
- **WHEN** a `VariantPriceChangedEvent` domain event is raised
- **THEN** a domain event subscriber SHALL publish a `VariantPriceChanged` integration event to RabbitMQ with old and new prices

### Requirement: Existing ProductUpdated integration event

A `ProductUpdated` integration event class SHALL be added to `ProductIntegrationEvents` in `EShop.Shared.Contracts` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `Name` (string), `Description` (string), `CategoryId` (Guid), `Tags` (string[]), `Slug` (string), `Images` (string[]), `Groups` (Guid[]).

#### Scenario: ProductUpdated integration event published
- **WHEN** a `ProductUpdatedEvent` domain event is raised
- **THEN** a domain event subscriber SHALL publish a `ProductUpdated` integration event to RabbitMQ
