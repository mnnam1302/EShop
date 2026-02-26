## ADDED Requirements

### Requirement: All services have healthchecks
Every service in docker-compose.yml SHALL define a `healthcheck` configuration.

#### Scenario: Service healthcheck presence
- **WHEN** a service is defined in docker-compose.yml
- **THEN** it SHALL include a `healthcheck` block with test, interval, timeout, retries, and start_period

### Requirement: HTTP services use wget for healthcheck
.NET services SHALL use `wget -q --spider http://localhost:8080/health` for healthcheck test command.

#### Scenario: .NET service healthcheck command
- **WHEN** a .NET service healthcheck runs
- **THEN** it SHALL execute `wget -q --spider http://localhost:8080/health || exit 1`

### Requirement: Alpine images include wget
All Dockerfiles using Alpine base images SHALL install wget via `apk add --no-cache wget`.

#### Scenario: Dockerfile wget installation
- **WHEN** building an Alpine-based .NET image
- **THEN** the base stage SHALL include `RUN apk add --no-cache wget`

### Requirement: Healthcheck timing configuration
Healthchecks SHALL use standardized timing: interval=30s, timeout=10s, retries=3, start_period=30s.

#### Scenario: Healthcheck timing values
- **WHEN** a service healthcheck is configured
- **THEN** interval SHALL be 30s, timeout SHALL be 10s, retries SHALL be 3, start_period SHALL be 30s

### Requirement: Infrastructure services use native healthchecks
PostgreSQL, Redis, and RabbitMQ SHALL use their native healthcheck commands.

#### Scenario: PostgreSQL healthcheck
- **WHEN** PostgreSQL healthcheck runs
- **THEN** it SHALL use `pg_isready -U postgres`

#### Scenario: RabbitMQ healthcheck
- **WHEN** RabbitMQ healthcheck runs
- **THEN** it SHALL use `rabbitmq-diagnostics check_port_connectivity`

### Requirement: Service dependencies use health condition
Services SHALL use `depends_on` with `condition: service_healthy` for services with healthchecks.

#### Scenario: Healthy dependency condition
- **WHEN** a service depends on another service with healthcheck
- **THEN** it SHALL use `condition: service_healthy` in depends_on
