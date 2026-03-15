## ADDED Requirements

### Requirement: CreateCategory end-to-end BDD verification
The BDD test SHALL verify the full CQRS loop for creating a category: HTTP POST → command handler → event store → integration event → consumer → MongoDB projection.

#### Scenario: Created category appears in MongoDB read model
- **WHEN** a system user with `Catalog_ManageCategories` permission creates a category with Name "Electronics", Reference "ELEC123", Slug "electronics"
- **THEN** the MongoDB `Category` collection SHALL contain a document with matching Name, Reference, Slug, TenantId, CreatedAtUtc, and UpdatedAtUtc

#### Scenario: Created category with parent appears in MongoDB read model
- **WHEN** a system user creates a parent category "Electronics" (Reference "ELEC123")
- **AND** then creates a child category "Laptops" (Reference "LAP456") with ParentId set to the parent's ID
- **THEN** the MongoDB `Category` collection SHALL contain a document for "Laptops" with the correct ParentId referencing the parent category

#### Scenario: Category projection includes TenantId
- **WHEN** a system user in tenant "TEST-TENANT" creates a category
- **THEN** the MongoDB `Category` document SHALL have `TenantId` equal to "TEST-TENANT"

### Requirement: UpdateCategory end-to-end BDD verification
The BDD test SHALL verify the full CQRS loop for updating a category through to the MongoDB projection.

#### Scenario: Updated category is reflected in MongoDB read model
- **WHEN** a system user creates a category with Name "Electronics" and Reference "ELEC123"
- **AND** then updates the category's Name to "Consumer Electronics"
- **THEN** the MongoDB `Category` document for Reference "ELEC123" SHALL have Name "Consumer Electronics" and an updated `UpdatedAtUtc` timestamp

### Requirement: Direct MongoDB query for Then steps
The `Then` step definitions SHALL verify projections by querying MongoDB directly via `IMongoRepositoryBase<Category>`, not via HTTP GET endpoints.

#### Scenario: StepContext queries MongoDB by Reference
- **WHEN** a `Then` step needs to verify a category's projected data
- **THEN** it SHALL resolve `IMongoRepositoryBase<Category>` from `ApiContext.ServiceProvider` and query by the category's Reference field

#### Scenario: StepContext asserts projected fields match expectations
- **WHEN** a `Then` step compares expected data from a DataTable against the MongoDB document
- **THEN** it SHALL assert that Name, Reference, Slug, and ParentId match the expected values

### Requirement: Async consumer settling before verification
The BDD infrastructure SHALL ensure all MassTransit consumers (including MongoDB projection consumers) have completed before executing `Then` step assertions.

#### Scenario: AfterStep waits for consumers before Then
- **WHEN** a `When` step publishes an integration event (directly or indirectly via a command)
- **THEN** the `[AfterStep]` hook SHALL call `WaitForQuietAsync()` which waits for read-side consumers to finish before the next step executes

#### Scenario: Cascading consumer settling
- **WHEN** a write-side command handler publishes a `CategoryCreated` integration event
- **AND** the `CategoryCreatedConsumer` (read-side) processes it and creates the MongoDB projection
- **THEN** `WaitForQuietAsync()` SHALL not return until the read-side consumer has fully completed

### Requirement: Idempotent consumer verification
The BDD test SHALL verify that the idempotent consumer pattern prevents duplicate projections when the same integration event is consumed multiple times.

#### Scenario: Duplicate event does not create duplicate projection
- **WHEN** a `CategoryCreated` integration event is published
- **AND** the same event (same EventId) is published again
- **THEN** the MongoDB `Category` collection SHALL contain exactly one document for that category
- **AND** the `InboxMessage` collection SHALL contain an entry marking the event as processed
