## ADDED Requirements

### Requirement: Infrastructure provisionable via both Docker Compose and Aspire
The system SHALL allow the full local development infrastructure — PostgreSQL, Redis, RabbitMQ, MongoDB — and the observability stack to be provisioned via either plain `docker compose` or the Aspire AppHost (`EShop.AppHost`), with no functional difference in the resulting environment.

#### Scenario: Provision via Docker Compose
- **WHEN** a developer runs `docker compose -f deploy/docker/docker-compose.infra.dev.yml up -d`
- **THEN** PostgreSQL, Redis, RabbitMQ, MongoDB, and the observability stack SHALL all start and report healthy

#### Scenario: Provision via Aspire
- **WHEN** a developer runs `dotnet run --project EShop.AppHost` in default (non-external) mode
- **THEN** PostgreSQL, Redis, RabbitMQ, MongoDB, and the observability stack SHALL all be provisioned as Aspire resources

### Requirement: Consistent infrastructure image versions across both paths
The system SHALL pin the same container image tag for each infrastructure component in both the Docker Compose files and the Aspire AppHost.

#### Scenario: RabbitMQ version parity
- **WHEN** RabbitMQ is provisioned via Docker Compose or via Aspire
- **THEN** both SHALL use the same RabbitMQ 3.x management image tag

#### Scenario: Datastore version parity
- **WHEN** PostgreSQL, Redis, or MongoDB is provisioned via either path
- **THEN** both paths SHALL use PostgreSQL `17.0`, Redis `7.4.7`, and MongoDB `6.0`

### Requirement: Observability stack available in both paths
The system SHALL provision the OpenTelemetry Collector, Prometheus, Grafana, and Node Exporter in both the Docker Compose dev path and the Aspire path.

#### Scenario: Observability via Docker Compose
- **WHEN** the dev infrastructure compose file is brought up
- **THEN** OpenTelemetry Collector, Prometheus, Grafana, and Node Exporter SHALL be running

#### Scenario: Observability via Aspire
- **WHEN** the Aspire AppHost runs in default mode
- **THEN** OpenTelemetry Collector, Prometheus, Grafana, and Node Exporter SHALL be provisioned as resources

### Requirement: Single source of truth for shared configuration
The system SHALL have both provisioning paths reference the same configuration files under `deploy/config/**` (OpenTelemetry Collector, Prometheus, Grafana) and the same database init scripts under `deploy/scripts/`, rather than maintaining duplicate copies.

#### Scenario: Shared OTel collector config
- **WHEN** the OpenTelemetry Collector is provisioned via either path
- **THEN** it SHALL load configuration from `deploy/config/otelcollector/config.yaml`

#### Scenario: Shared database init scripts
- **WHEN** PostgreSQL is provisioned via either path
- **THEN** it SHALL execute the init scripts from `deploy/scripts/`

### Requirement: Persistent data volumes with consistent names
The system SHALL use named persistent volumes for stateful infrastructure, with the same volume naming used across paths and free of naming defects.

#### Scenario: Postgres volume name corrected
- **WHEN** PostgreSQL is provisioned
- **THEN** its data volume SHALL be named `eshop-data` (not the previous misspelled `ehop-data`)

### Requirement: No committed secrets in infrastructure configuration
The system SHALL NOT contain any hardcoded secret value (such as the OpenTelemetry Dashboard API key) in the Aspire AppHost or in any committed Docker Compose or config file.

#### Scenario: No hardcoded OTel dashboard key
- **WHEN** the repository is scanned for the previously committed OTel Dashboard API key value
- **THEN** no source-controlled file SHALL contain that literal value
