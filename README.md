# EShop Kodi Project

## Setup infrastructure local
> Run docker-compose to build Infrastructure such as Redis, MSSQL Server, RabbitMQ, Seq for development environment.
```
docker-compose -f docker-compose.Dev.Infrastructure.yml up -d
```

## Design

### Domain Driven Design

### Event Sourcing

### Clean Architecture

## Best pratices and supports
### Cross Cutting Concern
| STT     | Name		                     |
|---------|----------------------------------|
| 1       | Authentication and Authorization |
| 2       | Logging and tracing              |
| 3       | Exception Handling               |
| 4       | Validation                       |
| 5       | Caching                          |

### Multi-tenancy

### Unit testing and Behavior driven design

### Trace Logs for Audit and Business

## Central Package Management to enhance migration
```
# Powershell script. Scan all .csproj files and aggregate unique package versions
$packages = Get-ChildItem -Filter *.csproj -Recurse |
    Get-Content |
    Select-String -Pattern '<PackageReference Include="([^"]+)" Version="([^"]+)"' -AllMatches |
    ForEach-Object { $_.Matches } |
    Group-Object { $_.Groups[1].Value } |
    ForEach-Object { @{
        Name = $_.Name
        Versions = $_.Group.ForEach({ $_.Groups[2].Value }) | Select-Object -Unique
    }} |
    Sort-Object { $_.Name }

# Display results
$packages | ForEach-Object {
    "$($_.Name) versions:"
    $_.Versions | ForEach-Object { "  $_" }
}

# Centralized package management to ensure consistent library versions across entry projects.
```

## Identity

### Service Overview
The Identity service is responsible for authentication, authorization, and user management across the EShop platform. It provides a centralized system for managing organizations in a hierarchical structure, users, roles, and permissions. The service supports multi-tenancy and maintains the security boundaries between different organizations.

### Ubiquitous language
Organization: A legal entity that forms the tenant boundary, potentially hierarchical
User: An individual who accesses the system with authentication credentials
Role: A collection of permissions that can be assigned to users
Permission: A specific action that can be performed within the system
TenantId: Unique identifier for top-level organizations that define tenant boundaries
OrganizationContext: Hierarchical position information for an organization

### Aggregates
- Organization Aggregate
  - Root: Organization
  - Entities: User
  - Value Objects: OrganizationContext
  - Invariants: OrganizationNumber must be unique, Organization hierarchy cannot exceed MaxSupportedLevel

- User Aggregate
  - Root: User
  - Entities: UserRole
  - Value Objects: PasswordHash, Email
  - Invariants: Username cannot contain special characters, Email must be valid

- Role Aggregate
  - Root: Role
  - Entities: RolePermission
  - Value Objects: None
  - Invariants: Role name must be unique within an organization

### Authentication Flow
1. User submits credentials (username/password)
2. Service validates credentials against stored PasswordHash
3. Upon successful authentication, JWT token is generated with claims:
   - User identifier
   - Username
   - Organization identifier
   - Roles and permissions
4. Token is used for subsequent authenticated requests
5. Authorization is enforced based on user roles and permissions

### API Capabilities
- Organization Management
  - Create/Update/Delete organizations
  - Create child organizations (hierarchical structure)
- User Management
  - Register/Authenticate users
  - Assign users to organizations
  - Activate/Deactivate users
- Role & Permission Management
  - Create/Update/Delete roles
  - Assign permissions to roles
  - Grant/Revoke user roles

### Integration Events
UserEvents : Integrations

- Organization integration events
- User integration events
- Role integration events


## Tenancy

### Service Overview
The Tenancy service manages multi-tenant capabilities across the EShop platform. It's responsible for tenant lifecycle management, feature flag configuration, and tenant-specific settings. This service enables the SaaS platform to maintain proper isolation between tenants while providing flexible feature configuration.                  └────────────────────┘
```

### Ubiquitous language
Tenant: An isolated instance of the application serving an organization
TenantFeature: A specific capability enabled or disabled for a tenant
Feature: A system capability that can be toggled per tenant
Module: A functional area of the system that groups features
FeatureState: Whether a feature is Enabled, Disabled, or any custom state
Category: A grouping for features to organize them logically

### Aggregates
- Tenant Aggregate
  - Root: Tenant
  - Entities: TenantFeature
  - Value Objects: None
  - Invariants: Tenant ID can only contain letters, digits, dashes, and underscores
    
- Feature Aggregate
  - Root: Feature
  - Entities: None
  - Value Objects: None
  - Invariants: Feature name must be unique across the system

### Key Functionality
- Tenant Management
  - Create/Update tenants
  - Manage tenant lifecycle (provision, suspend, archive)
- Feature Management
  - Define system features and their default states
  - Group features by module and category
- Tenant Feature Configuration
  - Enable/disable features per tenant
  - Override system defaults for specific tenants
  - Audit feature state changes

### Integration Events
```
TenantCreated : TenantEvents {
	TenantId
	OrganizationId
	Name
	DataIsolationStrategy
	CreatedAt
	CreatedBy
	InitialPlanId
	Status
	TenantConnectionString
}
```