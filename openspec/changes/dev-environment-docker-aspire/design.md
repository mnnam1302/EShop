## Context

The repo offers two ways to run locally, and they have drifted:

- **Aspire** (`EShop.AppHost/Extensions/ExternalServiceRegistrationExtensions.cs`): in default (non-external) mode it provisions the full observability stack (OTel Collector, Prometheus, Grafana, Node Exporter), Postgres 17 (6 databases), MongoDB 6, Redis 7.4.7, and RabbitMQ **4.1** — but only Tenancy and Authorization are wired as projects; Catalog (write/read), Inventory, Order, Finance, and the API Gateway are commented out (lines 181-273). It requires the .NET SDK + Aspire workload.
- **docker-compose** (`deploy/docker/`): `docker-compose.infra.dev.yml` provides Postgres/Redis/RabbitMQ/MongoDB (+ postgres-exporter) but **no observability stack** and RabbitMQ **3-management-alpine**. `docker-compose.dev.yml` containerizes Tenancy, Authorization, and the API Gateway via per-service Dockerfiles — but Catalog/Inventory/Order/Finance have no Dockerfiles and aren't in compose. A legacy root `docker-compose.yml` + `docker-compose.override.yml` duplicate an all-in-one dev stack.

Two facts make this tractable:
1. **Migrate-on-boot already exists**: every service has a `DbInitializer` that calls `Database.MigrateAsync()` (Tenancy/Authorization/Inventory/Order/Finance/Catalog). No new migration mechanism is needed — only consistent invocation at startup.
2. **Shared config already exists** under `deploy/config/**` (otelcollector, prometheus, grafana) and `deploy/scripts/` (DB init). Both paths can point at the same files instead of duplicating.

Locked decisions (from proposal review): migrate-on-boot, RabbitMQ **3.x** for both paths, consolidate compose into `deploy/docker/` (delete the legacy root files).

## Goals / Non-Goals

**Goals:**
- A contributor with only Docker + git can run the **entire** system (all 7 services + API Gateway + infra + observability) via a single `docker compose ... up` — no .NET SDK required.
- The same system runs via `dotnet run --project EShop.AppHost` (Aspire), with all services restored.
- Versions, ports, credentials, and config files are a single source of truth shared by both paths — no drift.
- No secrets committed to source; fresh clones self-serve from committed `*.template` files.
- DB schema is applied automatically on first run in both paths.

**Non-Goals:**
- No changes to production compose files beyond keeping network/secret names consistent.
- No application/domain logic changes (only Dockerfiles, compose, AppHost wiring, config, docs).
- No CI/CD pipeline or cloud-deployment work.
- Not introducing a new migration framework or a migrator container (migrate-on-boot is reused).

## Decisions

### D1: Auto-migration via migrate-on-service-boot (reuse existing `DbInitializer`)
Each service already runs `DbInitializer.Initialize()` → `MigrateAsync()` at startup. We standardize this: every service invokes its initializer during boot, gated to run in the dev environment, before serving traffic. Postgres/Mongo readiness is guaranteed by compose `depends_on: condition: service_healthy` and Aspire `WaitFor(...)`.
- **Why not a one-shot migrator container?** It would duplicate migration logic outside the services and add a container per path; the in-repo pattern already covers it. Migrate-on-boot keeps a new dev's mental model to "the service brings up its own schema."
- **Trade-off**: with multiple replicas, migrations could race. Dev runs a single replica per service, so this is acceptable; concurrent `MigrateAsync` is also advisory-locked by EF/Postgres. Documented as a dev-only assumption.

### D2: Standardize RabbitMQ on 3.x for both paths
Change Aspire `.WithImageTag("4.1")` → the 3.x management tag matching `deploy/docker/docker-compose.infra.dev.yml` (`3-management-alpine`). One broker version everywhere.
- **Why 3.x over 4.1?** Conservative: matches the version compose/dev tooling is already validated against; avoids a major-version migration in this change. Revisitable later as its own change.

### D3: Consolidate all compose files under `deploy/docker/`; delete legacy root compose
Remove root `docker-compose.yml` and `docker-compose.override.yml`. The canonical dev stack becomes the two-file overlay already established:
- `docker-compose.infra.dev.yml` — infra **+ observability stack** (added: otel-collector, prometheus, grafana, node-exporter using `deploy/config/**`).
- `docker-compose.dev.yml` — **all** app services (add Catalog write, Catalog read, Inventory, Order, Finance to the existing Tenancy/Authorization/Gateway).
- **Why?** A single source of truth eliminates the drift between root and `deploy/docker/`. Cost: the convenient bare `docker compose up` at repo root goes away; mitigated by documenting the exact command (and optionally a `Makefile`/`compose.yaml` thin wrapper if desired later).

### D4: Per-service multi-stage Dockerfiles so the host needs only Docker
Create Dockerfiles for Catalog (Application write side), Catalog (ReadModels.MongoDb), Inventory, Order, Finance, mirroring the existing Tenancy/Authorization/Gateway Dockerfiles (SDK build stage → `dotnet publish` → runtime image, `context: ../..`). Build happens **inside** the container, so no .NET SDK on the host.
- **Why mirror existing?** Consistency and zero novel build infra; the three working Dockerfiles are the template.

### D5: No committed secrets — env + secret-file templates
Remove the hardcoded OTel Dashboard API key from `EShop.AppHost` and any compose/config file. Source it (and existing DB/broker secrets) from gitignored files with committed `*.template` siblings under `Deployment/secrets/` (extends the existing `docker-secrets` capability). Aspire reads from user-secrets/env; compose reads from `deploy/secrets/*.txt` + `deploy/env/dev.env`. The Getting Started doc shows the one `cp *.template` step.

### D6: Shared config is the single source of truth
Both paths bind-mount/point at the same `deploy/config/otelcollector/config.yaml`, `deploy/config/prometheus/*.yml`, `deploy/config/grafana/**`, and `deploy/scripts/` DB init files. Compose observability services and Aspire `AddContainer(...)` use identical image tags (Prometheus `v3.5.0`, node-exporter `v1.8.2`, Grafana, Postgres `17.0`, Mongo `6.0`, Redis `7.4.7`, RabbitMQ 3.x). Fix the `ehop-data` volume typo → `eshop-data` consistently.

## Risks / Trade-offs

- **Migration race across replicas** → Dev uses single replicas; EF/Postgres advisory locks serialize concurrent `MigrateAsync`. Documented as a dev assumption; not for prod scale-out.
- **Full-stack compose is heavy on low-RAM machines** (7 services + 4 infra + 4 observability) → keep existing `deploy.resources` limits; document a "minimal" overlay (infra-only) for contributors who run services from the IDE.
- **Deleting root compose breaks muscle memory / scripts** → Call out the removal prominently in the proposal (BREAKING) and README; provide the exact replacement command.
- **First-run image build is slow** (multi-stage builds for 8 images) → expected one-time cost; layer caching makes subsequent runs fast. Documented.
- **RabbitMQ pinned to 3.x while 4.x exists** → conscious conservatism; note it as a future upgrade change so it isn't lost.
- **Two paths still must be kept in sync by humans** → mitigated by D6 (shared config files) and a checklist in `deploy/README.md`; full automated parity testing is out of scope.

## Migration Plan

1. Add Dockerfiles for the 4 missing services (D4); verify each builds standalone.
2. Add observability services to `docker-compose.infra.dev.yml`; add the 4 services to `docker-compose.dev.yml` (D3) pointing at shared config (D6).
3. Restore all commented services + API Gateway in `EShop.AppHost`; set RabbitMQ to 3.x; remove hardcoded key (D2/D5); fix volume typo.
4. Ensure migrate-on-boot is invoked + dev-gated in every service (D1).
5. Move secrets to `*.template` + gitignore; update `deploy/env/dev.env` (D5).
6. Delete root `docker-compose.yml` / `docker-compose.override.yml`; rewrite Getting Started docs (two commands, prerequisites, URLs).
7. Verify: fresh clone → `docker compose -f infra.dev -f dev up` brings the whole system healthy; separately `dotnet run --project EShop.AppHost` does the same.

**Rollback**: revert the change set; the legacy root compose and current AppHost wiring are restored from git history. No data migrations are destructive (dev volumes only).

## Open Questions

- Should we add a thin `compose.yaml`/`Makefile` at repo root that wraps the two-file `deploy/docker` overlay for ergonomic `docker compose up`? (Deferred — not required for correctness.)
- Catalog write side is EventFlow/event-sourced — confirm its boot-time schema setup matches the `DbInitializer.MigrateAsync` assumption or needs its own init path.
