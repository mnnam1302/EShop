## ADDED Requirements

### Requirement: ProductStateMachine permits Update as self-loop

The `ProductStateMachine` SHALL use `PermitReentry(ProductAction.Update)` in Draft, Published, and Unpublished states. The `Ignore(ProductAction.Update)` in Draft SHALL be replaced with `PermitReentry`. Published and Unpublished states SHALL also configure `PermitReentry(ProductAction.Update)`.

#### Scenario: Update fires in Draft state
- **WHEN** the Product is in Draft state and `State.Fire(ProductAction.Update)` is called
- **THEN** the state machine SHALL transition (reentry) and remain in Draft

#### Scenario: Update fires in Published state
- **WHEN** the Product is in Published state and `State.Fire(ProductAction.Update)` is called
- **THEN** the state machine SHALL transition (reentry) and remain in Published

#### Scenario: Update fires in Unpublished state
- **WHEN** the Product is in Unpublished state and `State.Fire(ProductAction.Update)` is called
- **THEN** the state machine SHALL transition (reentry) and remain in Unpublished

#### Scenario: CanFire(Update) returns true in non-Deleted states
- **WHEN** `State.CanFire(ProductAction.Update)` is queried for Draft, Published, or Unpublished
- **THEN** it SHALL return `true`

#### Scenario: CanFire(Update) returns false in Deleted state
- **WHEN** `State.CanFire(ProductAction.Update)` is queried for Deleted
- **THEN** it SHALL return `false`

### Requirement: Product can be published

The `ProductAggregate` SHALL expose a `Publish()` behavior method that validates the `ProductCanPublishSpecification` and raises a `ProductPublishedEvent`. The Apply handler SHALL fire `ProductAction.Publish` on the state machine.

#### Scenario: Publish a Draft product with valid state
- **WHEN** the Product is in Draft state, has at least one variant with Price > 0, and Name, Slug, and CategoryId are set
- **THEN** a `ProductPublishedEvent` SHALL be raised and the Product state SHALL transition to Published

#### Scenario: Publish an Unpublished product
- **WHEN** the Product is in Unpublished state and meets publish prerequisites
- **THEN** a `ProductPublishedEvent` SHALL be raised and the Product state SHALL transition to Published

#### Scenario: Publish fails when no variants exist
- **WHEN** the Product has zero variants and `Publish()` is called
- **THEN** a domain error SHALL be thrown indicating at least one variant is required

#### Scenario: Publish fails when no variant has a price
- **WHEN** all variants have Price ≤ 0 and `Publish()` is called
- **THEN** a domain error SHALL be thrown indicating at least one variant must have a price greater than zero

#### Scenario: Publish fails when Name is empty
- **WHEN** the Product Name is empty and `Publish()` is called
- **THEN** a domain error SHALL be thrown indicating Name is required

#### Scenario: Publish fails when Slug is empty
- **WHEN** the Product Slug is empty and `Publish()` is called
- **THEN** a domain error SHALL be thrown indicating Slug is required

#### Scenario: Publish fails when CategoryId is not set
- **WHEN** the Product CategoryId is `Guid.Empty` and `Publish()` is called
- **THEN** a domain error SHALL be thrown indicating CategoryId is required

#### Scenario: Publish fails when Product is already Published
- **WHEN** the Product state is Published and `Publish()` is called
- **THEN** a domain error SHALL be thrown because the state machine does not permit Publish from Published

#### Scenario: Publish fails when Product is Deleted
- **WHEN** the Product state is Deleted and `Publish()` is called
- **THEN** a domain error SHALL be thrown because the state machine does not permit Publish from Deleted

### Requirement: ProductCanPublishSpecification enforces publish prerequisites

The `ProductCanPublishSpecification` SHALL validate that:
1. The state machine allows the Publish action (`State.CanFire(ProductAction.Publish)`)
2. At least one variant exists in the Variants list
3. At least one variant has a Price greater than zero
4. Name is not null or empty
5. Slug is not null or empty
6. CategoryId is not `Guid.Empty`

#### Scenario: All prerequisites met
- **WHEN** the Product is in Draft, has one variant with Price = 29.99, Name = "T-Shirt", Slug = "t-shirt", CategoryId = valid Guid
- **THEN** the specification SHALL be satisfied

#### Scenario: Multiple violations reported
- **WHEN** the Product is in Draft with no variants and empty Name
- **THEN** the specification SHALL report all violations (missing variants AND missing name), not just the first one

### Requirement: Product can be unpublished

The `ProductAggregate` SHALL expose an `Unpublish()` behavior method that validates the `ProductCanUnpublishSpecification` and raises a `ProductUnpublishedEvent`. The Apply handler SHALL fire `ProductAction.Unpublish` on the state machine.

#### Scenario: Unpublish a Published product
- **WHEN** the Product is in Published state and `Unpublish()` is called
- **THEN** a `ProductUnpublishedEvent` SHALL be raised and the Product state SHALL transition to Unpublished

#### Scenario: Unpublish fails when Product is in Draft
- **WHEN** the Product is in Draft state and `Unpublish()` is called
- **THEN** a domain error SHALL be thrown because the state machine does not permit Unpublish from Draft

#### Scenario: Unpublish fails when Product is Deleted
- **WHEN** the Product is Deleted and `Unpublish()` is called
- **THEN** a domain error SHALL be thrown because the state machine does not permit Unpublish from Deleted

### Requirement: ProductCanUnpublishSpecification enforces unpublish prerequisites

The `ProductCanUnpublishSpecification` SHALL validate that the state machine allows the Unpublish action (`State.CanFire(ProductAction.Unpublish)`).

#### Scenario: Published product passes specification
- **WHEN** the Product is in Published state
- **THEN** the specification SHALL be satisfied

#### Scenario: Draft product fails specification
- **WHEN** the Product is in Draft state
- **THEN** the specification SHALL report that the Product cannot be unpublished in its current state

### Requirement: Product can be deleted

The `ProductAggregate` SHALL expose a `Delete()` behavior method that validates the `ProductCanDeleteSpecification` and raises a `ProductDeletedEvent`. The Apply handler SHALL fire `ProductAction.Delete` on the state machine.

#### Scenario: Delete a Draft product
- **WHEN** the Product is in Draft state and `Delete()` is called
- **THEN** a `ProductDeletedEvent` SHALL be raised and the Product state SHALL transition to Deleted

#### Scenario: Delete a Published product
- **WHEN** the Product is in Published state and `Delete()` is called
- **THEN** a `ProductDeletedEvent` SHALL be raised and the Product state SHALL transition to Deleted

#### Scenario: Delete an Unpublished product
- **WHEN** the Product is in Unpublished state and `Delete()` is called
- **THEN** a `ProductDeletedEvent` SHALL be raised and the Product state SHALL transition to Deleted

#### Scenario: Delete fails when Product is already Deleted
- **WHEN** the Product is in Deleted state and `Delete()` is called
- **THEN** a domain error SHALL be thrown because the state machine does not permit Delete from Deleted

### Requirement: ProductCanDeleteSpecification enforces delete prerequisites

The `ProductCanDeleteSpecification` SHALL validate that the state machine allows the Delete action (`State.CanFire(ProductAction.Delete)`).

#### Scenario: Non-deleted product passes specification
- **WHEN** the Product is in Draft, Published, or Unpublished state
- **THEN** the specification SHALL be satisfied

#### Scenario: Deleted product fails specification
- **WHEN** the Product is in Deleted state
- **THEN** the specification SHALL report that the Product cannot be deleted in its current state

### Requirement: ProductPublishedEvent domain event

The `ProductPublishedEvent` SHALL contain `ProductId` (Guid), `PublishedAtUtc` (DateTimeOffset), and `PublishedByUserId` (string). It SHALL be decorated with `[EventVersion]` following the existing naming convention.

#### Scenario: Event raised on publish
- **WHEN** `Publish()` is called and the specification is satisfied
- **THEN** a `ProductPublishedEvent` SHALL be raised with the correct ProductId, timestamp, and user ID

### Requirement: ProductUnpublishedEvent domain event

The `ProductUnpublishedEvent` SHALL contain `ProductId` (Guid), `UnpublishedAtUtc` (DateTimeOffset), and `UnpublishedByUserId` (string).

#### Scenario: Event raised on unpublish
- **WHEN** `Unpublish()` is called and the specification is satisfied
- **THEN** a `ProductUnpublishedEvent` SHALL be raised with the correct ProductId, timestamp, and user ID

### Requirement: ProductDeletedEvent domain event

The `ProductDeletedEvent` SHALL contain `ProductId` (Guid), `DeletedAtUtc` (DateTimeOffset), and `DeletedByUserId` (string).

#### Scenario: Event raised on delete
- **WHEN** `Delete()` is called and the specification is satisfied
- **THEN** a `ProductDeletedEvent` SHALL be raised with the correct ProductId, timestamp, and user ID

### Requirement: Publish vertical slice

A `Publish/` folder SHALL be created under `Products/` containing `PublishProductCommand`, `PublishProductCommandHandler`, `ProductPublishedEvent`, `ProductCanPublishSpecification`, and `PublishProductEndpointHandler`.

#### Scenario: Publish endpoint accepts POST
- **WHEN** a POST request is sent to the publish product endpoint with a valid product ID
- **THEN** the endpoint handler SHALL create a `PublishProductCommand`, publish it via `ICommandBus`, and return an appropriate response

#### Scenario: Command handler loads aggregate and calls Publish
- **WHEN** `PublishProductCommandHandler` handles a `PublishProductCommand`
- **THEN** it SHALL load the `ProductAggregate` from the event store, call `Publish()`, and persist the new events

### Requirement: Unpublish vertical slice

An `Unpublish/` folder SHALL be created under `Products/` containing `UnpublishProductCommand`, `UnpublishProductCommandHandler`, `ProductUnpublishedEvent`, `ProductCanUnpublishSpecification`, and `UnpublishProductEndpointHandler`.

#### Scenario: Unpublish endpoint accepts POST
- **WHEN** a POST request is sent to the unpublish product endpoint with a valid product ID
- **THEN** the endpoint handler SHALL create an `UnpublishProductCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: Delete vertical slice

A `Delete/` folder SHALL be created under `Products/` containing `DeleteProductCommand`, `DeleteProductCommandHandler`, `ProductDeletedEvent`, `ProductCanDeleteSpecification`, and `DeleteProductEndpointHandler`.

#### Scenario: Delete endpoint accepts POST
- **WHEN** a POST request is sent to the delete product endpoint with a valid product ID
- **THEN** the endpoint handler SHALL create a `DeleteProductCommand`, publish it via `ICommandBus`, and return an appropriate response

### Requirement: Integration events for product lifecycle

Three new integration event classes SHALL be added to `ProductIntegrationEvents` in `EShop.Shared.Contracts`:
- `ProductPublished` extending `CatalogIntegrationEvent` with `ProductId` (Guid)
- `ProductUnpublished` extending `CatalogIntegrationEvent` with `ProductId` (Guid)
- `ProductDeleted` extending `CatalogIntegrationEvent` with `ProductId` (Guid)

Each domain event subscriber SHALL publish the corresponding integration event via `IEventBus`.

#### Scenario: ProductPublished integration event published
- **WHEN** a `ProductPublishedEvent` domain event is raised
- **THEN** a domain event subscriber SHALL publish a `ProductPublished` integration event to RabbitMQ

#### Scenario: ProductDeleted integration event published
- **WHEN** a `ProductDeletedEvent` domain event is raised
- **THEN** a domain event subscriber SHALL publish a `ProductDeleted` integration event to RabbitMQ
