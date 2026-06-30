## 1. Per-service Dockerfiles (Docker-only host)

- [ ] 1.1 Create a multi-stage Dockerfile for Catalog write side (`Catalog/src/EShop.Catalog.Application/Dockerfile`) mirroring the existing Tenancy/Authorization Dockerfiles (`context: ../..`)
- [ ] 1.2 Create a multi-stage Dockerfile for Catalog read model (`Catalog/src/EShop.Catalog.ReadModels.MongoDb/Dockerfile`)
- [ ] 1.3 Create a multi-stage Dockerfile for Inventory (`Inventory/src/EShop.Inventory.API/Dockerfile`)
- [ ] 1.4 Create a multi-stage Dockerfile for Order (`Order/src/EShop.Order.API/Dockerfile`)
- [ ] 1.5 Create a multi-stage Dockerfile for Finance (`Finance/src/EShop.Finance.API/Dockerfile`)
- [ ] 1.6 Verify each new Dockerfile builds standalone (`docker build -f <path> .`) with no host .NET SDK dependency

## 2. Auto-migration on boot

- [ ] 2.1 Confirm each service invokes its `DbInitializer.Initialize()` (→ `MigrateAsync`) during startup, dev-gated
- [ ] 2.2 Verify Catalog (EventFlow/event-sourced) write side initializes its schema correctly on boot (resolve design Open Question)
- [ ] 2.3 Verify a service against an empty database applies migrations before serving traffic (no manual `dotnet ef`)

## 3. Docker Compose — observability parity

- [ ] 3.1 Add OpenTelemetry Collector, Prometheus, Grafana, and Node Exporter to `deploy/docker/docker-compose.infra.dev.yml`, bind-mounting `deploy/config/**`
- [ ] 3.2 Align RabbitMQ to the 3.x management image tag (already 3.x in compose — confirm consistency)
- [ ] 3.3 Apply existing `resource-limits`, `service-healthchecks`, and `logging-config` conventions to the new observability services
- [ ] 3.4 Rename the Postgres data volume `ehop-data` → `eshop-data` (and update all references)

## 4. Docker Compose — full app stack

- [ ] 4.1 Add Catalog write, Catalog read, Inventory, Order, and Finance services to `deploy/docker/docker-compose.dev.yml` (env, ports, networks, healthcheck, depends_on, resources, logging) following the existing Tenancy/Authorization pattern
- [ ] 4.2 Wire each new service's DB connection string, Redis, and RabbitMQ via env/secrets consistent with existing services
- [ ] 4.3 Add the new services as upstreams in the API Gateway service env/routing
- [ ] 4.4 Verify `docker compose -f docker-compose.infra.dev.yml -f docker-compose.dev.yml up` brings the whole stack healthy

## 5. Aspire — full stack parity

- [ ] 5.1 Uncomment/restore Catalog write, Catalog read, Inventory, Order, Finance, and API Gateway in `EShop.AppHost/Extensions/ExternalServiceRegistrationExtensions.cs` with their `WithReference`/`WaitFor` wiring
- [ ] 5.2 Change RabbitMQ `.WithImageTag("4.1")` → 3.x to match compose
- [ ] 5.3 Fix the Postgres data volume name `ehop-data` → `eshop-data`
- [ ] 5.4 Verify `dotnet run --project EShop.AppHost` brings up all services + infra + observability healthy

## 6. Secrets — no committed secrets

- [ ] 6.1 Remove the hardcoded OTel Dashboard API key from `EShop.AppHost` and any compose/config file
- [ ] 6.2 Source the OTel Dashboard API key from a gitignored secret file / env var in both paths
- [ ] 6.3 Add committed `*.template` files for every required secret; ensure real secret files are gitignored
- [ ] 6.4 Verify a repo scan finds no committed secret literals

## 7. Consolidate compose layout

- [ ] 7.1 Delete the legacy root `docker-compose.yml` and `docker-compose.override.yml`
- [ ] 7.2 Confirm no scripts/docs still reference the deleted root compose files; update any that do
- [ ] 7.3 Confirm `deploy/docker/*` is the single source of truth for dev and prod

## 8. Verification (fresh-clone smoke test)

- [ ] 8.1 From a clean clone with only Docker installed, run the documented compose command and confirm the full system reaches healthy
- [ ] 8.2 From a clean clone with the .NET SDK + Aspire workload, run the AppHost and confirm the full system reaches healthy
- [ ] 8.3 Confirm both paths expose the documented access URLs (gateway, services, dashboards)

## 9. Documentation (final step)

- [ ] 9.1 Rewrite `deploy/README.md` Getting Started to present exactly two supported entry points — the single Docker Compose command and the single Aspire command — with prerequisites per path
- [ ] 9.2 Document the one-time secret setup step (copy `*.template` files) and the access URLs (API Gateway, services, RabbitMQ UI, Grafana, Aspire dashboard, Prometheus)
- [ ] 9.3 Document the consolidated compose layout (`deploy/docker/*`) and note removal of the root compose files (BREAKING) with the replacement command
- [ ] 9.4 Document the RabbitMQ 3.x pin and the "future upgrade to 4.x" note from design
- [ ] 9.5 Update the root `README.md` Quick Start to point to the rewritten `deploy/README.md` flow
- [ ] 9.6 Add a short "keep both paths in sync" checklist (shared config, image tags) to `deploy/README.md`
