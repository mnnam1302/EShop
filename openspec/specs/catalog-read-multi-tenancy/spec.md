## ADDED Requirements

### Requirement: Global query filter for tenant isolation

The `CatalogReadDbContext` SHALL apply an EF Core global query filter on all entities implementing `IScoped`, filtering by `entity.TenantId == currentTenantId`. This filter SHALL be applied automatically to every query executed through the DbContext, including queries from JADNC's `EntityFrameworkCoreRepository`.

#### Scenario: JADNC query returns only current tenant's data
- **WHEN** a JSON:API GET request for categories is received from an authenticated user belonging to tenant "tenant-A"
- **THEN** the response SHALL contain only categories where `TenantId == "tenant-A"`

#### Scenario: Different tenants see different data
- **WHEN** tenant "tenant-A" has 5 categories and tenant "tenant-B" has 3 categories
- **THEN** a request from tenant-A SHALL return 5 categories and a request from tenant-B SHALL return 3 categories

#### Scenario: InboxMessage excluded from tenant filter
- **WHEN** the `CatalogReadDbContext` applies global query filters
- **THEN** the `InboxMessage` entity (which implements `IExcludedFromScoping`) SHALL NOT have a tenant filter applied, allowing cross-tenant idempotency checks

### Requirement: ITenantProvider resolves current tenant from request context

The system SHALL provide an `ITenantProvider` service that resolves the current tenant identifier from the HTTP request context. The implementation SHALL use the existing `IUserDetailsProvider.AuthenticatedUser.TenantId` to retrieve the tenant ID.

#### Scenario: Authenticated request resolves tenant
- **WHEN** an authenticated HTTP request is received with a valid user context
- **THEN** `ITenantProvider.TenantId` SHALL return the authenticated user's tenant ID

#### Scenario: Unauthenticated request returns empty tenant
- **WHEN** an unauthenticated HTTP request is received (no user context)
- **THEN** `ITenantProvider.TenantId` SHALL return null or empty string, causing the global query filter to return no tenant-scoped results

### Requirement: CatalogReadDbContext receives tenant context via ITenantProvider

The `CatalogReadDbContext` SHALL accept `ITenantProvider` as a constructor dependency and use its `TenantId` value when configuring global query filters in `OnModelCreating`. The tenant ID SHALL be captured into a field that the query filter expression references.

#### Scenario: DbContext constructed with tenant context
- **WHEN** a scoped `CatalogReadDbContext` is resolved from DI during an HTTP request
- **THEN** it SHALL receive the current `ITenantProvider` and apply the correct tenant's filter value

#### Scenario: Projection handlers operate with event's tenant context
- **WHEN** a MassTransit consumer processes an integration event containing a `TenantId`
- **THEN** the `ITenantProvider` SHALL be set to the event's tenant ID so that the `CatalogReadDbContext` operates in the correct tenant context for the projection

### Requirement: Multi-tenancy follows JADNC documented pattern

The multi-tenancy implementation SHALL follow the [JsonApiDotNetCore multi-tenancy pattern](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/docs/usage/advanced/multi-tenancy.md), ensuring compatibility with JADNC's query pipeline. The DbContext's global query filter SHALL be the sole mechanism for tenant filtering — no additional filtering SHALL be required in controllers, resource definitions, or custom repositories.

#### Scenario: No tenant filtering in controller code
- **WHEN** `CategoriesController` handles a GET request
- **THEN** the controller SHALL NOT contain any explicit tenant filtering logic — tenant isolation SHALL be handled entirely by the DbContext's global query filter

#### Scenario: JADNC filter and sort operations respect tenant scope
- **WHEN** a JSON:API request includes filter parameters (e.g., `?filter=equals(name,'Electronics')`)
- **THEN** the filter SHALL be applied within the tenant scope — only matching resources belonging to the current tenant SHALL be returned

#### Scenario: JADNC pagination counts reflect tenant-scoped data
- **WHEN** a JSON:API request includes pagination and `IncludeTotalResourceCount` is enabled
- **THEN** the total resource count SHALL reflect only the current tenant's resources, not all tenants

### Requirement: Tenant context available for event consumer projections

The system SHALL ensure that when MassTransit consumers process integration events, the `ITenantProvider` is populated with the tenant ID from the event payload. This allows projection handlers to insert read models with the correct tenant context through the `CatalogReadDbContext`.

#### Scenario: Consumer sets tenant context from event
- **WHEN** a `CategoryCreatedConsumer` processes a `CategoryCreated` event with `TenantId = "tenant-A"`
- **THEN** the `ITenantProvider` SHALL reflect `TenantId = "tenant-A"` for the duration of that consumer's scope

#### Scenario: Projection writes include tenant ID
- **WHEN** a projection handler inserts a new `Category` read model
- **THEN** the `Category.TenantId` and `Category.Scope` properties SHALL be set from the integration event's tenant ID
