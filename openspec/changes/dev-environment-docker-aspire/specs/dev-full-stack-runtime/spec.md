## ADDED Requirements

### Requirement: Full solution runs via Docker Compose with Docker as the only prerequisite
The system SHALL allow a developer whose machine has only Docker installed (no .NET SDK) to run the complete solution — all microservices, the API Gateway, infrastructure, and observability — via Docker Compose.

#### Scenario: Run full stack with only Docker installed
- **WHEN** a developer with only Docker installed runs the documented compose command for the full dev stack
- **THEN** all microservices, the API Gateway, infrastructure, and observability containers SHALL build and start without requiring the .NET SDK on the host

#### Scenario: In-container build
- **WHEN** a microservice image is built via Docker Compose
- **THEN** the .NET build and publish SHALL occur inside a multi-stage Docker build, not on the host

### Requirement: Every microservice has a Dockerfile
The system SHALL provide a multi-stage Dockerfile for every microservice and the API Gateway: Tenancy, Authorization, Catalog (write side), Catalog (read model), Inventory, Order, Finance, and API Gateway.

#### Scenario: Dockerfile present per service
- **WHEN** the repository is inspected for Dockerfiles
- **THEN** each of the eight services/gateway SHALL have a Dockerfile referenced by its Docker Compose service definition

### Requirement: Full solution runs via Aspire
The system SHALL run the complete solution — all microservices and the API Gateway — when the Aspire AppHost is started.

#### Scenario: All services wired in AppHost
- **WHEN** `dotnet run --project EShop.AppHost` is executed in default mode
- **THEN** Tenancy, Authorization, Catalog (write), Catalog (read), Inventory, Order, Finance, and the API Gateway SHALL all be started as Aspire project resources

### Requirement: Database schema applied automatically on first run
The system SHALL apply each service's database schema automatically at service startup, so that a fresh clone requires no manual migration step.

#### Scenario: Migration on boot via either path
- **WHEN** a service starts for the first time in the dev environment (via Compose or Aspire) against an empty database
- **THEN** the service SHALL apply its pending EF Core migrations before serving traffic

#### Scenario: No manual EF command required
- **WHEN** a developer follows the Getting Started instructions from a fresh clone
- **THEN** the instructions SHALL NOT require running any manual `dotnet ef` command

### Requirement: Services start only after their dependencies are healthy
The system SHALL order startup so that each service waits for its required infrastructure (database, broker, cache) to be healthy before starting.

#### Scenario: Compose dependency ordering
- **WHEN** the full stack is started via Docker Compose
- **THEN** each service SHALL declare `depends_on` with `condition: service_healthy` for its required infrastructure

#### Scenario: Aspire dependency ordering
- **WHEN** the full stack is started via Aspire
- **THEN** each service resource SHALL declare `WaitFor(...)` on its required infrastructure resources

### Requirement: Documented single Getting Started flow
The system SHALL document exactly two supported entry points — Docker Compose and Aspire — including prerequisites per path and the resulting access URLs.

#### Scenario: Getting Started lists both paths
- **WHEN** a new developer reads the Getting Started documentation
- **THEN** it SHALL present the single Docker Compose command and the single Aspire command, the prerequisites for each, and the service/dashboard access URLs
