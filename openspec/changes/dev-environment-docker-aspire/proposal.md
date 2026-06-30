## Why

A new developer who clones this repo today cannot get it running smoothly. There are two parallel ways to stand things up and both are incomplete and drifted:

- **Aspire (`EShop.AppHost`)** only wires up Tenancy + Authorization — Catalog (write/read), Inventory, Order, Finance, and the API Gateway are all commented out. It also requires the full .NET SDK + workloads installed.
- **docker-compose** is split across a legacy all-in-one root file and modular `deploy/docker/*` files; the dev infra file has no observability stack, runs a different RabbitMQ major version than Aspire (`3.x` vs `4.1`), and only the same two services are containerized.

On top of that, a hardcoded OTel Dashboard API key is committed in both paths. The net effect: the "getting started" experience depends on which path you guess, neither runs the whole system, and the two definitions silently disagree.

The goal of this change: **a developer who only knows how to install Docker and clone a Git repo can run the entire solution (infrastructure + all microservices) two ways — `docker compose up` or .NET Aspire — with no other prerequisites and no configuration drift between the two.**

## What Changes

- **Full-stack docker-compose path (Docker-only, no .NET SDK required)**: extend the dev compose setup so `docker compose up` builds and runs *all* microservices (Tenancy, Authorization, Catalog write, Catalog read, Inventory, Order, Finance, API Gateway) plus all infrastructure (Postgres, Redis, RabbitMQ, MongoDB) and the observability stack — entirely in containers. Each service gets a multi-stage `Dockerfile` (build + publish in-container) so the host needs only Docker.
- **Full-stack Aspire path**: uncomment/restore all microservices in `EShop.AppHost` so `dotnet run --project EShop.AppHost` brings up the complete system with the dashboard.
- **Single source of truth for infra config**: align RabbitMQ image tag, and reuse the same `deploy/config/**` (OTel collector, Prometheus, Grafana) and DB init scripts across both paths so versions/ports/credentials cannot drift. Add the observability stack to the docker-compose dev path to match Aspire.
- **No committed secrets**: remove the hardcoded OTel Dashboard API key (and any other committed secret) from both `EShop.AppHost` and compose files; source from gitignored secret/env files with committed `*.template` examples, following the existing `docker-secrets` convention.
- **Automated first-run**: ensure DB schema/migrations are applied automatically on startup in both paths (migration step on service boot or a one-shot migrator container) so a fresh clone needs no manual `dotnet ef` step.
- **One clear Getting Started doc** presenting exactly two supported commands (compose vs Aspire), prerequisites per path, and access URLs.
- **BREAKING**: the dev docker-compose topology changes (new service + observability containers, aligned RabbitMQ version, new secret/env template files); existing local `.env`/secret setups must be regenerated from the new templates.

## Capabilities

### New Capabilities
- `dev-infra-provisioning`: Local infrastructure (Postgres, Redis, RabbitMQ, MongoDB) and the observability stack (OTel Collector, Prometheus, Grafana, Node Exporter) are provisionable via both `docker compose` and Aspire, with matching versions/config/credentials and no committed secrets.
- `dev-full-stack-runtime`: The complete solution — all microservices plus the API Gateway — runs end-to-end via two supported entry points (`docker compose up` for a Docker-only host, and `dotnet run` on the Aspire AppHost), with automatic database migration on first run.

### Modified Capabilities
- `docker-secrets`: extend the secret-file convention to cover non-credential dev secrets (e.g. OTel Dashboard API key) and to provide committed `*.template` files so a fresh clone can self-serve.

## Impact

- **New/changed files**: per-service `Dockerfile`s for every microservice + API Gateway (multi-stage); `deploy/docker/docker-compose.dev.yml` (extended to all services); `deploy/docker/docker-compose.infra.dev.yml` (observability stack added); `EShop.AppHost/Program.cs` + extensions (all services restored, hardcoded key removed); `deploy/config/**` reused; `Deployment/secrets/*.template`; root `README.md` + `deploy/README.md`.
- **Cross-cutting**: requires a consistent in-container health/readiness contract and startup migration strategy across services; must keep `IScoped`/multi-tenant DbContext rules intact when adding auto-migration.
- **Out of scope**: production compose files (`*.prod.yml`) change only as needed to stay consistent with the new infra network/secret names; no application/domain logic changes.
- **Dependencies**: Docker Desktop (Compose v2) for the compose path; .NET 8 SDK + Aspire workload for the Aspire path only.
