# Getting Started — Local Development

Run the **entire** EShop stack (all microservices + API Gateway + infrastructure +
observability) locally in one of two supported ways:

| Path | Prerequisites | Best for |
|------|---------------|----------|
| **A. Docker Compose** | Docker Desktop only | Just want it running; no .NET toolchain |
| **B. .NET Aspire** | .NET 8 SDK + Aspire workload + Docker | Day-to-day service development & debugging |

Both paths use the **same** infrastructure versions, the **same** shared config under
`deploy/config/**`, apply database schema **automatically on first run**, and require
**no secret files** for dev. Pick one and go.

---

## Path A — Docker Compose (Docker is the only prerequisite)

A fresh clone needs nothing but Docker. Every service is built inside a multi-stage
container, so no .NET SDK is required on the host.

```bash
git clone https://github.com/mnnam1302/EShop.git
cd EShop

# Build + start the full stack (infra + observability + all services)
docker compose \
  -f deploy/docker/docker-compose.infra.dev.yml \
  -f deploy/docker/docker-compose.dev.yml \
  up -d --build
```

That's it. The `eshop-public` / `eshop-internal` networks are created automatically,
databases/users are created by `deploy/scripts/postgres-init.sql`, and each service
applies its EF Core migrations on startup before serving traffic — **no manual
`dotnet ef` step**.

Check status / stop:

```bash
docker compose -f deploy/docker/docker-compose.infra.dev.yml -f deploy/docker/docker-compose.dev.yml ps
docker compose -f deploy/docker/docker-compose.infra.dev.yml -f deploy/docker/docker-compose.dev.yml down
```

> First build pulls base images and compiles all services — expect a few minutes.
> Subsequent runs are fast (layer cache); drop `--build` to skip rebuilding.

### Infrastructure only (run services from your IDE)

If you'd rather run the .NET services yourself, start just the infrastructure +
observability:

```bash
docker compose -f deploy/docker/docker-compose.infra.dev.yml up -d
docker compose -f deploy/docker/docker-compose.infra.dev.yml down
```

Then run a service against it, e.g.:

```bash
dotnet run --project Tenancy/src/EShop.Tenancy.API
```

(The services default to `localhost` infra; connection strings use the per-service
Postgres users created by `postgres-init.sql`, e.g.
`Username=tenancy;Password=tenancy-password-dev;Server=localhost;Port=5432;Database=eshop_tenancy`.)

---

## Path B — .NET Aspire

Aspire orchestrates the same infrastructure, observability, and all services, with a
live dependency dashboard.

```bash
# One-time: install the Aspire workload
dotnet workload install aspire

git clone https://github.com/mnnam1302/EShop.git
cd EShop

dotnet run --project EShop.AppHost
```

Aspire provisions PostgreSQL, MongoDB, Redis, RabbitMQ, the OpenTelemetry Collector,
Prometheus, Grafana and Node Exporter as containers, then launches every service
project. The Aspire dashboard URL is printed in the console on startup.

---

## Access points

| Service | URL | Notes |
|---------|-----|-------|
| API Gateway | http://localhost:5000 | routes to Tenancy + Authorization |
| Tenancy API | http://localhost:5001 | |
| Authorization API | http://localhost:5002 | |
| Catalog (write) | http://localhost:5003 | event-sourced |
| Catalog (read) | http://localhost:5004 | MongoDB projection |
| Inventory API | http://localhost:5005 | |
| Order API | http://localhost:5006 | |
| Finance API | http://localhost:5007 | |
| Aspire Dashboard | http://localhost:18888 | traces / logs / metrics (unsecured in dev) |
| Grafana | http://localhost:3000 | dashboards provisioned from `deploy/config/grafana` |
| Prometheus | http://localhost:9090 | |
| RabbitMQ Management | http://localhost:15672 | `guest` / `guest` |
| PostgreSQL | localhost:5432 | `postgres` / `postgres-dev` |
| MongoDB | localhost:27017 | `sa` / see infra compose |
| Redis | localhost:6379 | no auth (dev) |

> Health endpoint for every service: `GET /health`.

---

## Folder structure

```
deploy/
  docker/
    docker-compose.infra.dev.yml   ← dev infra + observability (plain env, no auth) — creates the networks
    docker-compose.dev.yml         ← all application services (dev, plain env, no secret files)
    docker-compose.infra.prod.yml  ← prod infra (Docker secrets)
    docker-compose.prod.yml        ← prod application services (Docker secrets)
  config/                          ← single source of truth, shared by Compose AND Aspire
    otelcollector/config.yaml
    prometheus/prometheus_pull.yml
    grafana/**
  env/
    dev.env                        ← committed, NON-secret dev config (OTLP endpoint, broker user/vhost)
    prod.env                       ← non-secret prod config (template)
  scripts/
    postgres-init.sql              ← creates DB users + databases on first run
    mongodb-init.js                ← creates Mongo users on first run
  secrets/
    *.txt.example                  ← prod secret templates — copy to *.txt and fill in
    *.txt                          ← real prod secrets — git-ignored, never commit
```

### Why no secret files in dev?

Dev infrastructure uses plain, well-known credentials (matching
`docker-compose.infra.dev.yml`) and the Aspire Dashboard runs **unsecured**
(`DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS`), so there is nothing secret to manage.
The `deploy/secrets/*.txt.example` templates and `deploy/secrets/*.txt` (git-ignored)
are used **only** by the production compose files.

---

## Versions & keeping the two paths in sync

Both paths pin the same images. When changing any of these, update **both**
`docker-compose.infra.dev.yml` and `EShop.AppHost`:

| Component | Image / tag |
|-----------|-------------|
| PostgreSQL | `postgres:17.0` |
| MongoDB | `mongo:6.0` |
| Redis | `redis:7.4.7` |
| RabbitMQ | `rabbitmq:3-management-alpine` (Aspire: tag `3` + management plugin) |
| Prometheus | `prom/prometheus:v3.5.0` |
| Node Exporter | `prom/node-exporter:v1.8.2` |
| OTel Collector | `...opentelemetry-collector-contrib:0.135.0` |

> **RabbitMQ is pinned to 3.x** for both paths. Upgrading to 4.x is deliberately left
> as a separate, dedicated change.

---

## Troubleshooting

**Containers unhealthy** — inspect logs:

```bash
docker logs postgres --tail 50
docker logs rabbitmq --tail 50
docker compose -f deploy/docker/docker-compose.infra.dev.yml -f deploy/docker/docker-compose.dev.yml logs <service> --tail 80
```

**Port already in use** (e.g. 5432) — stop the conflicting process or change the host
port mapping in `docker/docker-compose.dev.yml`:

```powershell
netstat -ano | findstr :5432
```

**`network eshop-internal exists but was not created by compose`** — you have leftover
networks from the old (pre-consolidation) setup. Remove them once and re-run:

```bash
docker network rm eshop-internal eshop-public
```

**Reset everything (clean slate — destroys all data):**

```bash
docker compose \
  -f deploy/docker/docker-compose.infra.dev.yml \
  -f deploy/docker/docker-compose.dev.yml \
  down -v
```

Schema is re-created automatically on the next startup.

> **Note:** the legacy root `docker-compose.yml` / `docker-compose.override.yml` have
> been removed. Use the `deploy/docker/*` files above as the single source of truth.
