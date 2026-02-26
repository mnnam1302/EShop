## Context

The EShop solution uses Docker Compose for local development and deployment, with three main services (ApiGateway, Tenancy, Authorization) plus supporting infrastructure (PostgreSQL, Redis, RabbitMQ, Aspire Dashboard). The current configuration has bugs preventing deployment and lacks production-ready patterns for security, reliability, and resource management.

Current state issues:
- ApiGateway Dockerfile has broken syntax (Stage 4 `FROM` merged with comment)
- PostgreSQL secret uses wrong environment variable format
- ApiGateway service in docker-compose.yml is a stub with no configuration
- No resource limits on any container
- Inconsistent healthcheck patterns
- Secrets stored in environment variables

## Goals / Non-Goals

**Goals:**
- Fix all critical bugs preventing deployment
- Establish secure secret management using Docker secrets
- Standardize healthcheck configuration across all services
- Add resource limits to prevent container resource exhaustion
- Separate development and production configurations
- Create consistent logging configuration

**Non-Goals:**
- Kubernetes migration or Helm charts
- External secret management (HashiCorp Vault)
- CI/CD pipeline configuration
- Production deployment to cloud providers
- SSL/TLS certificate management
- Container registry configuration

## Decisions

### D1: Use Docker Secrets for Sensitive Data

**Decision:** All passwords and secrets stored as Docker secrets mounted at `/run/secrets/<name>`.

**Rationale:**
- Native Docker feature, no external dependencies
- Secrets stored in files, not environment variables (prevents accidental logging)
- Secrets are only accessible to services that explicitly request them

**Alternatives Considered:**
- Environment files (.env) - Less secure, can be accidentally committed or logged
- External vault (HashiCorp Vault) - Overkill for local dev, adds infrastructure complexity

**Implementation:**
```yaml
secrets:
  postgres_password:
    file: ./Deployment/secrets/postgres_password.txt
  tenancy_db_password:
    file: ./Deployment/secrets/tenancy_db_password.txt
```

Services read secrets using `_FILE` suffix environment variables where supported (PostgreSQL) or direct file read in application code.

### D2: Healthcheck Strategy Using wget

**Decision:** Use `wget -q --spider` for HTTP healthchecks on all .NET services.

**Rationale:**
- Alpine images don't include curl by default
- wget is smaller than curl
- `--spider` mode only checks headers, doesn't download body

**Alternatives Considered:**
- Add curl via `apk add` - Increases image size unnecessarily
- Use .NET's built-in health endpoint with `dotnet` CLI - No lightweight CLI tool available
- PowerShell healthcheck - Not available in Alpine

**Configuration:**
```yaml
healthcheck:
  test: ["CMD-SHELL", "wget -q --spider http://localhost:8080/health || exit 1"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 30s
```

### D3: Resource Limits Based on Service Tier

**Decision:** Three tiers of resource allocation based on service role.

| Tier | Services | Memory Limit | CPU Limit |
|------|----------|--------------|-----------|
| Light | ApiGateway, Dashboard, Redis | 256M | 0.5 |
| Standard | Tenancy, Authorization, RabbitMQ | 512M | 1.0 |
| Database | PostgreSQL | 512M | 1.0 |

**Rationale:**
- ApiGateway is a proxy with minimal processing - needs less resources
- Business services (Tenancy, Authorization) need more headroom for queries and processing
- PostgreSQL needs guaranteed resources for concurrent connections

**Alternatives Considered:**
- No limits - Risk of container consuming all host resources
- Higher limits - Wasteful for local development
- Per-service tuning - Premature optimization without metrics

### D4: Docker Compose Override for Development

**Decision:** Use `docker-compose.override.yml` (auto-loaded) for development-specific configuration.

**Rationale:**
- Docker Compose automatically merges `docker-compose.override.yml` when present
- Keeps production-ready base config clean
- No need to specify `-f` flags for local development

**What goes in override:**
- Port mappings to localhost (`:5001:8080`)
- Volume mounts for user secrets
- `ASPNETCORE_ENVIRONMENT=Development`
- Lower resource limits for dev machines

**What stays in base:**
- Service definitions
- Network configuration
- Secret references
- Healthchecks
- Production environment defaults

### D5: Logging Configuration with Rotation

**Decision:** Use json-file driver with 10MB max size and 3 file rotation.

**Rationale:**
- Default json-file driver is sufficient for local development
- Rotation prevents disk exhaustion from long-running containers
- 30MB total per service (10MB × 3 files) is reasonable for debugging

**Configuration:**
```yaml
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

## Risks / Trade-offs

### R1: Secret File Management
**Risk:** Secret files in `Deployment/secrets/` could be committed to git.
**Mitigation:** Add `Deployment/secrets/*.txt` to `.gitignore`. Create `.txt.example` files with placeholder values.

### R2: Healthcheck Start Period
**Risk:** 30-second start period may be too short for slow-starting services on low-resource machines.
**Mitigation:** Override start_period in docker-compose.override.yml if needed. Monitor startup times.

### R3: Resource Limits Too Restrictive
**Risk:** 512M memory limit may be insufficient for large datasets or memory-intensive operations.
**Mitigation:** Start conservative, increase based on actual usage. Document how to adjust in override file.

### R4: Alpine Image Compatibility
**Risk:** Alpine uses musl libc instead of glibc, which can cause issues with some .NET features.
**Mitigation:** Already tested with current services. Add `RUN apk add --no-cache icu-libs` if globalization issues occur.

### R5: Breaking Change for Existing Workflows
**Risk:** Developers with existing docker-compose usage may have conflicts.
**Mitigation:** Document migration steps. Secret files are new, not replacing existing configuration.
