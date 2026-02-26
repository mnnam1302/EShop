## ADDED Requirements

### Requirement: All services have resource limits
Every service in docker-compose.yml SHALL define `deploy.resources.limits` for memory and CPU.

#### Scenario: Resource limits presence
- **WHEN** a service is defined in docker-compose.yml
- **THEN** it SHALL include `deploy.resources.limits` with `memory` and `cpus` values

### Requirement: Resource reservations defined
Services SHALL define `deploy.resources.reservations` to guarantee minimum resources.

#### Scenario: Resource reservations presence
- **WHEN** a service has resource limits
- **THEN** it SHALL also have `reservations` with `memory` and `cpus` values

### Requirement: Light tier services
ApiGateway, Dashboard, and Redis SHALL use light tier resources: memory limit 256M, CPU limit 0.5.

#### Scenario: ApiGateway resources
- **WHEN** ApiGateway is deployed
- **THEN** it SHALL have memory limit of 256M and CPU limit of 0.5

#### Scenario: Redis resources
- **WHEN** Redis is deployed
- **THEN** it SHALL have memory limit of 256M (or 128M) and CPU limit of 0.5 (or 0.25)

### Requirement: Standard tier services
Tenancy, Authorization, and RabbitMQ SHALL use standard tier resources: memory limit 512M, CPU limit 1.0.

#### Scenario: Tenancy resources
- **WHEN** Tenancy service is deployed
- **THEN** it SHALL have memory limit of 512M and CPU limit of 1.0

#### Scenario: Authorization resources
- **WHEN** Authorization service is deployed
- **THEN** it SHALL have memory limit of 512M and CPU limit of 1.0

### Requirement: Database tier services
PostgreSQL SHALL use database tier resources: memory limit 512M, CPU limit 1.0.

#### Scenario: PostgreSQL resources
- **WHEN** PostgreSQL is deployed
- **THEN** it SHALL have memory limit of 512M and CPU limit of 1.0

### Requirement: Reservations are proportional to limits
Resource reservations SHALL be approximately 50% of limits (memory) and 25% of limits (CPU).

#### Scenario: Reservation proportions
- **WHEN** a service has 512M memory limit
- **THEN** it SHALL have approximately 256M memory reservation
