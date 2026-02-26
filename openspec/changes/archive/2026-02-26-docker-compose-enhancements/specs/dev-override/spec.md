## ADDED Requirements

### Requirement: Development override file exists
The system SHALL have a `docker-compose.override.yml` file for development-specific configuration.

#### Scenario: Override file presence
- **WHEN** running docker-compose in the project root
- **THEN** `docker-compose.override.yml` SHALL exist and be auto-loaded

### Requirement: Override file auto-loading
Docker Compose SHALL automatically merge `docker-compose.override.yml` with the base `docker-compose.yml`.

#### Scenario: Auto-merge behavior
- **WHEN** `docker-compose up` is executed without `-f` flag
- **THEN** both `docker-compose.yml` and `docker-compose.override.yml` SHALL be merged

### Requirement: Port mappings in override
All localhost port mappings (`:XXXX:8080`) SHALL be defined in the override file, not the base file.

#### Scenario: Port mapping location
- **WHEN** exposing service ports to localhost
- **THEN** the `ports:` section SHALL be in `docker-compose.override.yml`

#### Scenario: Tenancy port mapping
- **WHEN** accessing Tenancy locally
- **THEN** it SHALL be available on port 5001

#### Scenario: Authorization port mapping
- **WHEN** accessing Authorization locally
- **THEN** it SHALL be available on port 5002

#### Scenario: ApiGateway port mapping
- **WHEN** accessing ApiGateway locally
- **THEN** it SHALL be available on port 5000

### Requirement: Development environment variables
The override file SHALL set `ASPNETCORE_ENVIRONMENT=Development` for .NET services.

#### Scenario: Development environment
- **WHEN** running locally via docker-compose
- **THEN** .NET services SHALL have `ASPNETCORE_ENVIRONMENT` set to `Development`

### Requirement: User secrets volume mounts
The override file SHALL mount user secrets directories for .NET services during development.

#### Scenario: User secrets access
- **WHEN** a .NET service runs in development
- **THEN** `${APPDATA}/Microsoft/UserSecrets` SHALL be mounted as read-only volume

### Requirement: Base file is production-ready
The base `docker-compose.yml` SHALL contain production-ready defaults without development-specific configuration.

#### Scenario: Production defaults
- **WHEN** deploying without override file
- **THEN** services SHALL run with production-appropriate configuration
