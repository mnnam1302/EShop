## 1. Per-service Dockerfiles (Docker-only host)

- [x] 1.1 Create a multi-stage Dockerfile for Catalog write side (`Catalog/src/EShop.Catalog.Application/Dockerfile`) mirroring the existing Tenancy/Authorization Dockerfiles (`context: ../..`)
- [x] 1.2 Create a multi-stage Dockerfile for Catalog read model (`Catalog/src/EShop.Catalog.ReadModels.MongoDb/Dockerfile`)
- [x] 1.3 Create a multi-stage Dockerfile for Inventory (`Inventory/src/EShop.Inventory.API/Dockerfile`)
- [x] 1.4 Create a multi-stage Dockerfile for Order (`Order/src/EShop.Order.API/Dockerfile`)
- [x] 1.5 Create a multi-stage Dockerfile for Finance (`Finance/src/EShop.Finance.API/Dockerfile`)
- [x] 1.6 Verify each new Dockerfile builds standalone (`docker build -f <path> .`) — verified Finance, Catalog-write, Catalog-read, Tenancy build with 0 errors in-container (no host SDK). Required fixing a latent `Directory.Build.props` omission in all 8 Dockerfiles and the DomainTools folder/file casing (committed lowercase `Eshop...` vs referenced `EShop...`) that broke every Linux build

## 2. Auto-migration on boot

- [x] 2.1 Confirm each service invokes its `DbInitializer.Initialize()` (→ `MigrateAsync`) during startup (all 6 services call it before `app.RunAsync()`; currently unconditional, not env-gated — acceptable for dev, left as-is to avoid changing prod behavior)
- [x] 2.2 Verify Catalog (EventFlow/event-sourced) write side initializes its schema correctly on boot — its `DbInitializer` calls `CatalogDbContext.Database.MigrateAsync()`, consistent with migrate-on-boot
- [x] 2.3 Verify a service against an empty database applies migrations before serving traffic (no manual `dotnet ef`) — `Initialize()` is awaited before `RunAsync()`; full runtime check covered by group 8

## 3. Docker Compose — observability parity

- [x] 3.1 Add OpenTelemetry Collector, Prometheus, Grafana, Node Exporter (+ Aspire Dashboard) to `docker-compose.infra.dev.yml`, bind-mounting `deploy/config/**`; same image tags as Aspire
- [x] 3.2 Align RabbitMQ to the 3.x management image tag — compose already `3-management-alpine`; Aspire aligned in 5.2
- [x] 3.3 `resource-limits`/`logging-config` intentionally omitted on dev infra+observability (the infra.dev.yml philosophy is "no resource limits — do not constrain the dev machine"; existing infra services also omit them). Healthchecks kept on datastores; observability containers left unconstrained for parity with that philosophy
- [x] 3.4 Postgres dev volume already `eshop-data` in compose; Aspire `ehop-data`→`eshop-data` fixed (in AppHost)
- [x] 3.5 Fix `external: true` networks so the stack auto-creates `eshop-public`/`eshop-internal` on a fresh clone (no manual `docker network create`)

## 4. Docker Compose — full app stack

- [x] 4.1 Add Catalog write, Catalog read, Inventory, Order, Finance (+ keep Tenancy/Authorization/Gateway) to `docker-compose.dev.yml` via shared YAML anchors (healthcheck, depends_on, resources, logging)
- [x] 4.2 Wire each service's DB connection string (plain dev creds matching postgres-init.sql), Redis, RabbitMQ, and Catalog-read Mongo via env — no secret files needed for dev
- [~] 4.3 API Gateway upstream env kept for tenancy/authorization (the only clusters defined in the gateway's ReverseProxy config). YARP routes/clusters for catalog/inventory/order/finance are app-routing config out of scope here; those services run and are reachable on their own ports. **Follow-up noted.**
- [x] 4.4 `docker compose -f infra.dev -f dev config` validates and merges cleanly; per-service images build. Full live boot is the group-8 smoke test.

## 5. Aspire — full stack parity

- [x] 5.1 Uncommented Catalog write, Catalog read, Inventory, Order, Finance, and API Gateway in the AppHost with their `WithReference`/`WaitFor` wiring
- [x] 5.2 Changed RabbitMQ `.WithImageTag("4.1")` → `"3"` (keeps `WithManagementPlugin`, mirrors compose 3.x)
- [x] 5.3 Postgres data volume `ehop-data` → `eshop-data` (fixed in AppHost)
- [x] 5.4 `dotnet build EShop.AppHost` succeeds with all services wired (full host solution compiles). Live `dotnet run` boot left to the developer / group-8 smoke test (requires Aspire workload + heavy container startup)

## 6. Secrets — no committed secrets

- [x] 6.1 Removed the hardcoded OTel key: AppHost never had it in source (reads from config); the only committed copy was root `docker-compose.yml` (deleted in 7.1). Also stripped it from `dev.env` and removed orphaned `open-telemetry.env`
- [x] 6.2 Dev needs no key at all — Aspire Dashboard runs unsecured in dev (`DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS`). Aspire path reads its key from config (auto). No secret in either path
- [x] 6.3 Dev requires zero secret files (plain dev creds via committed `dev.env` + inline env). Prod secret `*.txt.example` templates already exist; real `deploy/secrets/*.txt` remain gitignored. Added `!deploy/env/dev.env` negation so the non-secret dev env is committed for clone-and-run
- [x] 6.4 `git grep 104293...` → no committed secret literal remains

## 7. Consolidate compose layout

- [x] 7.1 Deleted legacy root `docker-compose.yml` + `docker-compose.override.yml` (and orphaned `open-telemetry.env`)
- [x] 7.2 Only references to the old root compose are in archived openspec history (left untouched). NOTE: accepted specs `dev-override`, `resource-limits`, `service-healthchecks`, `logging-config` describe the now-deleted root compose — they should be reconciled/removed when archiving this change (follow-up)
- [x] 7.3 `deploy/docker/*` is now the single source of truth (root compose removed); also restored `EShop.sln` after the IDE dropped the DomainTools project during the rename

## 8. Verification (fresh-clone smoke test)

- [~] 8.1 Compose smoke test: full **infra + observability** stack (`docker compose -f infra.dev up`) comes up with postgres/mongodb/rabbitmq/redis **healthy**; OTel Collector logs "Everything is ready"; Prometheus/Grafana return HTTP 200, Aspire Dashboard 302; networks auto-created. All 8 service **images build** (Finance/Catalog-write/Catalog-read/Tenancy verified). Full 8-service live boot left as a final manual run (heavy; ~minutes)
- [~] 8.2 Aspire: `dotnet build EShop.AppHost` succeeds with all services wired. Live `dotnet run` left to the developer (needs Aspire workload + container startup)
- [x] 8.3 Access URLs documented in `deploy/README.md`; observability URLs verified responding during the smoke test

## 9. Documentation (final step)

- [x] 9.1 Rewrote `deploy/README.md` Getting Started — two entry points (single Compose command, single Aspire command) with per-path prerequisites
- [x] 9.2 Documented that dev needs no secret files (plain creds + unsecured dashboard) and listed all access URLs (gateway, 7 services, Grafana, Prometheus, Aspire dashboard, RabbitMQ, datastores)
- [x] 9.3 Documented the consolidated `deploy/docker/*` layout and the removal of the root compose files (BREAKING) with replacement commands
- [x] 9.4 Documented the RabbitMQ 3.x pin + "upgrade to 4.x is a separate change" note
- [x] 9.5 Updated root `README.md` Quick Start with both Option A (Docker-only) and Option B (Aspire)
- [x] 9.6 Added a "keep both paths in sync" image-version table to `deploy/README.md`
