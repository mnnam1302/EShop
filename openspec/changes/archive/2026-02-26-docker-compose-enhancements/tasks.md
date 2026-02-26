## 1. Critical Bug Fixes

- [x] 1.1 Fix ApiGateway Dockerfile syntax error (line 54: add newline before `FROM base AS final`)
- [x] 1.2 Fix PostgreSQL secret handling (change `POSTGRES_PASSWORD` to `POSTGRES_PASSWORD_FILE`)

## 2. Secret Infrastructure Setup

- [x] 2.1 Create `Deployment/secrets/` directory structure
- [x] 2.2 Create secret file: `Deployment/secrets/postgres_password.txt`
- [x] 2.3 Create secret file: `Deployment/secrets/tenancy_db_password.txt`
- [x] 2.4 Create secret file: `Deployment/secrets/authorization_db_password.txt`
- [x] 2.5 Create secret file: `Deployment/secrets/rabbitmq_password.txt`
- [x] 2.6 Create secret file: `Deployment/secrets/jwt_secret.txt`
- [x] 2.7 Add `Deployment/secrets/*.txt` to `.gitignore`
- [x] 2.8 Create example secret files with placeholders (`.txt.example`)

## 3. Dockerfile Updates

- [x] 3.1 Add `RUN apk add --no-cache wget` to ApiGateway Dockerfile base stage
- [x] 3.2 Add `RUN apk add --no-cache wget` to Tenancy Dockerfile base stage
- [x] 3.3 Add `RUN apk add --no-cache wget` to Authorization Dockerfile base stage

## 4. Docker Compose Base Configuration

- [x] 4.1 Add secrets section with all five secret definitions
- [x] 4.2 Update PostgreSQL to use `POSTGRES_PASSWORD_FILE` environment variable
- [x] 4.3 Add secrets array to Tenancy service
- [x] 4.4 Add secrets array to Authorization service
- [x] 4.5 Add complete ApiGateway service definition with environment, healthcheck, depends_on, secrets, networks
- [x] 4.6 Add healthcheck to Redis service
- [x] 4.7 Update healthcheck timing for all services (interval=30s, timeout=10s, retries=3, start_period=30s)
- [x] 4.8 Add resource limits to all services (deploy.resources.limits)
- [x] 4.9 Add resource reservations to all services (deploy.resources.reservations)
- [x] 4.10 Add logging configuration to all services (json-file driver with rotation)
- [x] 4.11 Update restart policy to `unless-stopped` for all services
- [x] 4.12 Remove port mappings from base docker-compose.yml (move to override)
- [x] 4.13 Remove `ASPNETCORE_ENVIRONMENT=Development` from base (move to override)
- [x] 4.14 Remove UserSecrets volume mounts from base (move to override)

## 5. Docker Compose Override Creation

- [x] 5.1 Create `docker-compose.override.yml` file
- [x] 5.2 Add port mappings: Dashboard (18888), PostgreSQL (5432), Redis (6379), RabbitMQ (5672, 15672)
- [x] 5.3 Add port mappings: Tenancy (5001), Authorization (5002), ApiGateway (5000)
- [x] 5.4 Add `ASPNETCORE_ENVIRONMENT=Development` for .NET services
- [x] 5.5 Add UserSecrets volume mounts for .NET services

## 6. Validation

- [x] 6.1 Run `docker-compose config` to validate merged configuration
- [x] 6.2 Run `docker-compose build` to verify Dockerfile changes
- [ ] 6.3 Run `docker-compose up` and verify all services start healthy
- [ ] 6.4 Verify ApiGateway routes to Tenancy and Authorization correctly
