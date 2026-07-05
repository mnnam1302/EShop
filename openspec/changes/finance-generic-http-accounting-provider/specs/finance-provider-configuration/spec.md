## ADDED Requirements

### Requirement: Per-tenant accounting company provisioning and configuration
The system SHALL provision exactly one tenant-scoped `AccountingCompany` when a tenant is created (consuming `TenantCreated`), defaulting its provider type to `None`. An authorised user SHALL then be able to configure that company with a provider type identifier (e.g. `GenericHttp`), a YAML behaviour configuration, and connection details. The `AccountingCompany` SHALL be tenant-scoped (`IScoped`) and isolated from other tenants by the EF Core global query filter. Provisioning SHALL be idempotent.

#### Scenario: Company provisioned on tenant creation
- **WHEN** a `TenantCreated` event is consumed for a new tenant
- **THEN** the system persists exactly one tenant-scoped `AccountingCompany` with provider type `None`
- **AND** consuming the same event again does not create a second company

#### Scenario: Configure the tenant's accounting company
- **WHEN** an authorised user configures provider type `GenericHttp` with a YAML behaviour configuration and connection details for their tenant
- **THEN** the system updates the tenant's existing `AccountingCompany` in place
- **AND** the configuration is retrievable only within that tenant's scope

### Requirement: Credentials stored separately and never disclosed
The system SHALL store provider credentials (such as `clientSecret` and `password`) encrypted at rest and separately from the YAML behaviour configuration. Credentials MUST NOT be returned in any read/API response, and MUST NOT be required to appear in the YAML configuration.

#### Scenario: Credentials excluded from configuration reads
- **WHEN** a user retrieves a tenant's provider configuration
- **THEN** the response contains the provider type and non-secret connection details
- **AND** no credential value (client secret, password) is present in the response

#### Scenario: Credentials supplied via a dedicated save step
- **WHEN** a user provides credentials for the connection and saves them
- **THEN** the credentials are stored encrypted and associated with the tenant's provider configuration
- **AND** the YAML behaviour configuration remains free of credential values

### Requirement: Named-provider resolution with safe default
The system SHALL resolve the active `IAccountingIntegrationProvider` for a tenant by matching the tenant's configured provider type against the registered provider implementations by `Name`. When a tenant has no provider configured, resolution SHALL return a no-op `None` provider so downstream flows are inert.

#### Scenario: Resolve a configured provider
- **WHEN** the booking pipeline requests the provider for a tenant configured with type `GenericHttp`
- **THEN** the factory returns the `GenericHttp` provider implementation

#### Scenario: Unconfigured tenant resolves to no-op
- **WHEN** the booking pipeline requests the provider for a tenant with no registered provider
- **THEN** the factory returns the `None` provider
- **AND** no external call is attempted

#### Scenario: Unknown provider type is rejected
- **WHEN** resolution is requested for a provider type that has no registered implementation
- **THEN** the system raises an error identifying the unknown provider type

### Requirement: Connection testing before use
The system SHALL provide a way to test a tenant's provider connection using the stored connection details, returning success or failure without performing any booking. When the configured authentication scheme has no verifiable token endpoint (e.g. Basic or NoAuth), the test SHALL be treated as successful.

#### Scenario: Successful OAuth connection test
- **WHEN** a user tests a connection whose credentials successfully obtain an access token from the configured token endpoint
- **THEN** the system reports the connection as valid

#### Scenario: Failed connection test
- **WHEN** a user tests a connection whose credentials are rejected by the token endpoint
- **THEN** the system reports the connection as invalid
- **AND** no exception is surfaced to the caller

#### Scenario: Non-verifiable scheme assumed valid
- **WHEN** a user tests a connection using Basic or NoAuth with no token endpoint to verify
- **THEN** the system reports the connection as valid
