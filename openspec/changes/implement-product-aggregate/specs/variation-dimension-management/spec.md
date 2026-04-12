## ADDED Requirements

### Requirement: Add variation dimension to a product

The `ProductAggregate` SHALL expose an `AddVariationDimension()` behavior method that validates the `CanAddVariationDimensionSpecification` and raises a `VariationDimensionAddedEvent`. The Apply handler SHALL add the new `VariationDimension` to the aggregate's `VariationDimensions` list.

#### Scenario: Add first dimension to a Draft product
- **WHEN** the Product is in Draft state with no existing dimensions and `AddVariationDimension("Color", "Color", ["Red", "Blue"], "Color")` is called
- **THEN** a `VariationDimensionAddedEvent` SHALL be raised and the VariationDimensions list SHALL contain one dimension named "Color" with values ["Red", "Blue"]

#### Scenario: Add second dimension to a product
- **WHEN** the Product already has a "Color" dimension and `AddVariationDimension("Size", "Size", ["S", "M", "L"], "Text")` is called
- **THEN** a `VariationDimensionAddedEvent` SHALL be raised and the VariationDimensions list SHALL contain two dimensions

#### Scenario: Add dimension to a Published product with no non-default variants
- **WHEN** the Product is Published, has only a default variant (no dimension values), and `AddVariationDimension()` is called
- **THEN** a `VariationDimensionAddedEvent` SHALL be raised

#### Scenario: Add dimension fails when product is Deleted
- **WHEN** the Product is Deleted and `AddVariationDimension()` is called
- **THEN** a domain error SHALL be thrown indicating the Product is in a Deleted state

### Requirement: CanAddVariationDimensionSpecification enforces dimension addition rules

The `CanAddVariationDimensionSpecification` SHALL validate that:
1. The Product is not in Deleted state
2. The dimension name is unique among existing VariationDimensions (case-insensitive)
3. At least one value is provided
4. Values are unique within the dimension (case-insensitive)
5. No non-default variants exist on the product

#### Scenario: Duplicate dimension name rejected
- **WHEN** the Product has a dimension named "Color" and `AddVariationDimension("Color", ...)` is called
- **THEN** a domain error SHALL be thrown indicating a dimension with name "Color" already exists

#### Scenario: Empty values list rejected
- **WHEN** `AddVariationDimension("Color", "Color", [], "Color")` is called with zero values
- **THEN** a domain error SHALL be thrown indicating at least one value is required

#### Scenario: Duplicate values within dimension rejected
- **WHEN** `AddVariationDimension("Color", "Color", ["Red", "Red"], "Color")` is called with duplicate values
- **THEN** a domain error SHALL be thrown indicating dimension values must be unique

#### Scenario: Non-default variants exist blocks addition
- **WHEN** the Product has a non-default variant (with dimension values) and `AddVariationDimension()` is called
- **THEN** a domain error SHALL be thrown indicating dimensions cannot be added when non-default variants exist

#### Scenario: Default-only variant allows addition
- **WHEN** the Product has only a default variant (no dimension values) and `AddVariationDimension()` is called with valid data
- **THEN** the specification SHALL be satisfied

### Requirement: VariationDimensionAddedEvent domain event

The `VariationDimensionAddedEvent` SHALL contain `ProductId` (Guid), `Name` (string), `DisplayName` (string), `Values` (string[]), and `DisplayStyle` (string).

#### Scenario: Event carries all dimension data
- **WHEN** a `VariationDimensionAddedEvent` is raised
- **THEN** it SHALL contain the dimension name, display name, all values, and the display style

### Requirement: Update variation dimension on a product

The `ProductAggregate` SHALL expose an `UpdateVariationDimension()` behavior method that validates the `CanUpdateVariationDimensionSpecification` and raises a `VariationDimensionUpdatedEvent`. The Apply handler SHALL update the matching dimension's `DisplayName` and `DisplayStyle` in the VariationDimensions list.

#### Scenario: Update display name of an existing dimension
- **WHEN** the Product has a "Color" dimension and `UpdateVariationDimension("Color", "Colour", "Color")` is called
- **THEN** a `VariationDimensionUpdatedEvent` SHALL be raised and the dimension's DisplayName SHALL become "Colour"

#### Scenario: Update display style of an existing dimension
- **WHEN** the Product has a "Color" dimension with DisplayStyle "Text" and `UpdateVariationDimension("Color", "Color", "Color")` is called
- **THEN** a `VariationDimensionUpdatedEvent` SHALL be raised and the dimension's DisplayStyle SHALL become "Color"

#### Scenario: Update fails when dimension does not exist
- **WHEN** `UpdateVariationDimension("Material", ...)` is called but no dimension named "Material" exists
- **THEN** a domain error SHALL be thrown indicating the dimension was not found

#### Scenario: Update fails when product is Deleted
- **WHEN** the Product is Deleted and `UpdateVariationDimension()` is called
- **THEN** a domain error SHALL be thrown

### Requirement: CanUpdateVariationDimensionSpecification enforces update rules

The `CanUpdateVariationDimensionSpecification` SHALL validate that:
1. The Product is not in Deleted state
2. A dimension with the specified name exists in VariationDimensions

#### Scenario: Existing dimension passes
- **WHEN** the Product has a dimension named "Color" and update is requested for "Color"
- **THEN** the specification SHALL be satisfied

#### Scenario: Non-existent dimension fails
- **WHEN** the Product has no dimension named "Material" and update is requested for "Material"
- **THEN** the specification SHALL report that the dimension does not exist

### Requirement: VariationDimensionUpdatedEvent domain event

The `VariationDimensionUpdatedEvent` SHALL contain `ProductId` (Guid), `Name` (string), `DisplayName` (string), and `DisplayStyle` (string).

#### Scenario: Event carries updated dimension metadata
- **WHEN** a `VariationDimensionUpdatedEvent` is raised
- **THEN** it SHALL contain the dimension name (identifier), the new display name, and the new display style

### Requirement: Change variation dimension values on a product

The `ProductAggregate` SHALL expose a `ChangeVariationDimensionValues()` behavior method that validates the `CanChangeVariationDimensionValuesSpecification` and raises a `VariationDimensionValuesChangedEvent`. The Apply handler SHALL replace the matching dimension's `Values` array with the new values.

#### Scenario: Add a new value to an existing dimension
- **WHEN** the "Color" dimension has values ["Red", "Blue"] and `ChangeVariationDimensionValues("Color", ["Red", "Blue", "Green"])` is called
- **THEN** a `VariationDimensionValuesChangedEvent` SHALL be raised and the dimension values SHALL become ["Red", "Blue", "Green"]

#### Scenario: Remove an unreferenced value
- **WHEN** the "Color" dimension has values ["Red", "Blue", "Green"], no variant references "Green", and `ChangeVariationDimensionValues("Color", ["Red", "Blue"])` is called
- **THEN** a `VariationDimensionValuesChangedEvent` SHALL be raised and the dimension values SHALL become ["Red", "Blue"]

#### Scenario: Remove a referenced value is blocked
- **WHEN** the "Color" dimension has values ["Red", "Blue"] and a non-deleted variant has DimensionValue "Blue", and `ChangeVariationDimensionValues("Color", ["Red"])` is called
- **THEN** a domain error SHALL be thrown indicating "Blue" is referenced by an existing variant

#### Scenario: Change fails when product is Deleted
- **WHEN** the Product is Deleted and `ChangeVariationDimensionValues()` is called
- **THEN** a domain error SHALL be thrown

### Requirement: CanChangeVariationDimensionValuesSpecification enforces value change rules

The `CanChangeVariationDimensionValuesSpecification` SHALL validate that:
1. The Product is not in Deleted state
2. A dimension with the specified name exists in VariationDimensions
3. At least one value is provided in the new values list
4. New values are unique within the list (case-insensitive)
5. No removed value (present in old values but absent in new values) is referenced by any non-deleted variant's `VariantDimensionValues`

#### Scenario: All values retained with addition passes
- **WHEN** old values are ["Red", "Blue"], new values are ["Red", "Blue", "Green"], and no variants are affected
- **THEN** the specification SHALL be satisfied

#### Scenario: Removed value referenced by variant fails
- **WHEN** old values are ["Red", "Blue"], new values are ["Red"], and a variant has DimensionValue "Blue"
- **THEN** the specification SHALL report that "Blue" cannot be removed because it is referenced by a variant

#### Scenario: Empty new values fails
- **WHEN** new values list is empty
- **THEN** the specification SHALL report that at least one value is required

#### Scenario: Duplicate new values fails
- **WHEN** new values are ["Red", "Red"]
- **THEN** the specification SHALL report that dimension values must be unique

### Requirement: VariationDimensionValuesChangedEvent domain event

The `VariationDimensionValuesChangedEvent` SHALL contain `ProductId` (Guid), `DimensionName` (string), and `Values` (string[]).

#### Scenario: Event carries the new values
- **WHEN** a `VariationDimensionValuesChangedEvent` is raised
- **THEN** it SHALL contain the dimension name and the complete new values array

### Requirement: AddVariationDimension vertical slice

An `AddVariationDimension/` folder SHALL be created under `Products/` containing `AddVariationDimensionCommand`, `AddVariationDimensionCommandHandler`, `VariationDimensionAddedEvent`, `CanAddVariationDimensionSpecification`, and `AddVariationDimensionEndpointHandler`.

#### Scenario: Endpoint accepts POST with dimension data
- **WHEN** a POST request is sent to the add-variation-dimension endpoint with product ID, dimension name, display name, values, and display style
- **THEN** the endpoint handler SHALL create an `AddVariationDimensionCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: UpdateVariationDimension vertical slice

An `UpdateVariationDimension/` folder SHALL be created under `Products/` containing `UpdateVariationDimensionCommand`, `UpdateVariationDimensionCommandHandler`, `VariationDimensionUpdatedEvent`, `CanUpdateVariationDimensionSpecification`, and `UpdateVariationDimensionEndpointHandler`.

#### Scenario: Endpoint accepts PUT with updated dimension data
- **WHEN** a PUT request is sent to the update-variation-dimension endpoint with product ID, dimension name, new display name, and new display style
- **THEN** the endpoint handler SHALL create an `UpdateVariationDimensionCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: ChangeVariationDimensionValues vertical slice

A `ChangeVariationDimensionValues/` folder SHALL be created under `Products/` containing `ChangeVariationDimensionValuesCommand`, `ChangeVariationDimensionValuesCommandHandler`, `VariationDimensionValuesChangedEvent`, `CanChangeVariationDimensionValuesSpecification`, and `ChangeVariationDimensionValuesEndpointHandler`.

#### Scenario: Endpoint accepts PUT with new values
- **WHEN** a PUT request is sent to the change-variation-dimension-values endpoint with product ID, dimension name, and new values array
- **THEN** the endpoint handler SHALL create a `ChangeVariationDimensionValuesCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: Integration events for variation dimension operations

Three new integration event classes SHALL be added to `ProductIntegrationEvents` in `EShop.Shared.Contracts`:
- `VariationDimensionAdded` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `Name` (string), `DisplayName` (string), `Values` (string[]), `DisplayStyle` (string)
- `VariationDimensionUpdated` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `Name` (string), `DisplayName` (string), `DisplayStyle` (string)
- `VariationDimensionValuesChanged` extending `CatalogIntegrationEvent` with `ProductId` (Guid), `DimensionName` (string), `Values` (string[])

Each domain event subscriber SHALL publish the corresponding integration event via `IEventBus`.

#### Scenario: VariationDimensionAdded integration event published
- **WHEN** a `VariationDimensionAddedEvent` domain event is raised
- **THEN** a domain event subscriber SHALL publish a `VariationDimensionAdded` integration event to RabbitMQ

#### Scenario: VariationDimensionValuesChanged integration event published
- **WHEN** a `VariationDimensionValuesChangedEvent` domain event is raised
- **THEN** a domain event subscriber SHALL publish a `VariationDimensionValuesChanged` integration event to RabbitMQ
