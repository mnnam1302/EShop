# Getting Started — Local Development

This guide walks you through everything needed to run the full EShop stack locally using Docker Compose.

---

## Folder structure

```
deploy/
  docker/
    docker-compose.infra.dev.yml        ← dev infra: postgres, redis, rabbitmq (plain env, no auth)
    docker-compose.infra.prod.yml       ← prod infra: postgres, redis, rabbitmq (Docker secrets)
    docker-compose.dev.yml              ← application services (dev)
    docker-compose.prod.yml             ← application services (prod)
  env/
    dev.env                             ← non-sensitive dev config
    prod.env                            ← non-sensitive prod config (template)
  scripts/
    postgres-init.sql                   ← creates DB users + databases on first run
  secrets/
    *.txt.example                       ← templates — copy to *.txt and fill in
    *.txt                               ← real secrets — git-ignored, never commit
```

---

## Prerequisites

| Tool | Minimum version | Install |
|------|----------------|---------|
| .NET SDK | 8.0 | https://dot.net/download |
| Docker Desktop | 4.x | https://docs.docker.com/desktop |
| Git | any | https://git-scm.com |

Verify your setup:

```bash
dotnet --version      # 8.x.x
docker --version
docker compose version
```

---

## 1. Clone the repository

```bash
git clone https://github.com/mnnam1302/EShop.git
cd EShop
```

---

## 2. Secret files

Dev infrastructure (postgres, redis, rabbitmq) runs with **plain env var credentials** —
no secret files are needed to start infra locally.

Secret files are only required when running **containerized app services** (the commented
services in `docker-compose.dev.yml`). Copy from the example templates if needed:

```powershell
$secrets = @("rabbitmq_password", "tenancy_connstr", "authorization_connstr")
foreach ($s in $secrets) {
    Copy-Item "deploy/secrets/$s.txt.example" "deploy/secrets/$s.txt"
}
```

| File | Used for |
|------|----------|
| `rabbitmq_password.txt` | RabbitMQ password for containerized app services |
| `tenancy_connstr.txt` | Tenancy DB connection string |
| `authorization_connstr.txt` | Authorization DB connection string |

> **Production** additionally requires `postgres_password.txt` and `redis_password.txt` —
> used by `docker-compose.infra.prod.yml` via Docker secrets.

---

## 3. Start infrastructure

Start PostgreSQL, Redis, RabbitMQ, and the Aspire observability dashboard:

```bash
docker compose -f deploy/docker/docker-compose.infra.dev.yml up -d
```

Verify all containers are healthy before continuing:

```bash
docker compose -f deploy/docker/docker-compose.infra.dev.yml ps
```

All services should show `healthy` or `running`.

---

## 4. Run database migrations

The `postgres-init.sql` script runs automatically the first time the postgres container starts — it creates each service's database user and empty database. EF Core migrations then create the schema tables inside those databases.

Run migrations for each service you need (from the **repository root**):

```bash
# Tenancy
dotnet ef database update \
  --project Tenancy/src/EShop.Tenancy.Infrastructure \
  --startup-project Tenancy/src/EShop.Tenancy.API

# Authorization
dotnet ef database update \
  --project Authorization/src/EShop.Authorization.Infrastructure \
  --startup-project Authorization/src/EShop.Authorization.API

# Inventory
dotnet ef database update \
  --project Inventory/src/EShop.Inventory.Infrastructure \
  --startup-project Inventory/src/EShop.Inventory.API
```

---

## 5. Run services locally (without Docker)

When running `dotnet run` outside a container, secrets are not at `/run/secrets/`. Supply config via environment variables instead:

```powershell
# Tenancy — PowerShell example
$env:ConnectionStrings__DefaultConnection  = "Username=tenancy;Password=tenancy-password-dev;Server=localhost;Port=5432;Database=eshop_tenancy"
$env:MasstransitConfiguration__Host       = "localhost"
$env:MasstransitConfiguration__Port       = "5672"
$env:MasstransitConfiguration__Username   = "guest"
$env:MasstransitConfiguration__Password   = "guest"
$env:MasstransitConfiguration__VirtualHost = "eshop01012025"
$env:RedisOptions__Host                  = "localhost:6379"
$env:ASPNETCORE_ENVIRONMENT               = "Development"

dotnet run --project Tenancy/src/EShop.Tenancy.API
```

---

## 6. Run everything in Docker (full stack)

Start infrastructure and all application services together:

```bash
docker compose -f deploy/docker/docker-compose.infra.dev.yml -f deploy/docker/docker-compose.dev.yml up -d --build
```

> `--build` rebuilds service images from source. Omit it on subsequent runs for faster startup.

Stop everything:

```bash
docker compose -f deploy/docker/docker-compose.infra.dev.yml -f deploy/docker/docker-compose.dev.yml down
```

---

## 7. Access points

| Service | URL | Credential |
|---------|-----|------------|
| **API Gateway** | http://localhost:5000 | — |
| **Tenancy API** | http://localhost:5001 | — (dev only) |
| **Authorization API** | http://localhost:5002 | — (dev only) |
| **Aspire Dashboard** | http://localhost:18888 | — |
| **RabbitMQ Management** | http://localhost:15672 | `guest` / `rabbitmq_password.txt` |
| **PostgreSQL** | localhost:5432 | `postgres` / `postgres_password.txt` |
| **Redis** | localhost:6379 | no auth (dev) |

---

## Troubleshooting

### Containers not starting / unhealthy

```bash
docker logs postgres  --tail 50
docker logs rabbitmq  --tail 50
docker logs redis     --tail 50
```

### `secret file not found`

Verify all secret `.txt` files exist (not just the `.example` files):

```powershell
Get-ChildItem deploy/secrets/*.txt
```

### EF migration fails — "entry point exited without ever building an IHost"

The startup project's DI container fails to build. Common cause: a service is registered that depends on something not yet configured (e.g. `IPublishEndpoint` without MassTransit). Check `DependencyInjection/ServiceCollectionExtensions.cs` in the Infrastructure project.

### Port already in use

```powershell
# Find what is using port 5432
netstat -ano | findstr :5432
```

Stop the conflicting process or change the host port mapping in `docker/docker-compose.dev.yml`.

### Reset everything (clean slate)

```bash
# Removes containers AND volumes — all database data will be lost
docker compose \
  -f deploy/docker/docker-compose.infra.dev.yml \
  -f deploy/docker/docker-compose.dev.yml \
  down -v
```

After this, re-run EF migrations (step 4) before starting services again.
