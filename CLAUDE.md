# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## The Four Principles

The following 4 core principles MUST be adhered to at all times.

### 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:

- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

### 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

### 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:

- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:

- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

### 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:

- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:

```text
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

## Project Overview

EShop is a multi-tenant SaaS e-commerce platform built as a .NET 8 microservices monorepo, demonstrating Clean Architecture, DDD, CQRS, and Event Sourcing. Services: **Tenancy**, **Authorization**, **Catalog**, **Inventory**, **Order**, plus a YARP-based **ApiGateway** (ReverseProxy) and shared cross-cutting libraries under `Shared/`. Orchestration/local dev runs via **.NET Aspire** (`EShop.AppHost`).

For Catalog's Product Aggregate (SPU/SKU, event storming, state machines, specifications) see `Catalog/src/EShop.Catalog.Application/README.md`. For Inventory's domain model, integration events, and data flows see `Inventory/src/EShop.Inventory.API/README.md`. For JWT/RSA key rotation internals see `Shared/src/EShop.Shared.Authentication/README.md`.

## Architecture

### Layering per service (Clean Architecture)

Most services (Inventory, Authorization, Tenancy, Order) follow:

```
<Service>/src/
  EShop.<Service>.Domain/           # Entities, aggregates, domain events, value objects, specifications — no outward deps
  EShop.<Service>.Application/      # Use cases (Commands/Queries + Handlers via MediatR), DI extensions
  EShop.<Service>.Infrastructure/   # EF Core DbContext, repositories, MassTransit consumers, migrations
  EShop.<Service>.API/              # Minimal API endpoints, Program.cs/Startup.cs, Swagger, validators
```

Dependency rule: `API → Infrastructure → Application → Domain` (Domain has no outward dependencies).

**Catalog** differs: `EShop.Catalog.Application` is a self-hosted, event-sourced write side (EventFlow-based, vertical slice — domain + CQRS in one project), and `EShop.Catalog.ReadModels.MongoDb` is a separate read-side service projecting to MongoDB via EF Core.

### CQRS / Event flow (write side)

1. Minimal API endpoint validates/maps request → Command
2. Command dispatched via `IMediator` (or EventFlow `ICommandBus` for Catalog) to a handler
3. Handler loads the aggregate, calls a behavior method
4. Aggregate checks a `Specification` (`ThrowDomainErrorIfNotSatisfied`), then raises a domain event and applies it to mutate state
5. Handler persists via repository (PostgreSQL event store for Catalog; EF Core tables for Inventory/Order/etc.)
6. Handler publishes an integration event via `IEventBus` (MassTransit → RabbitMQ)

### Read side / projections

Integration events are consumed by `IdempotentConsumer<TMessage>` (MassTransit), which checks a PostgreSQL `inbox_messages` table before dispatching a projection command (idempotency / inbox pattern). Catalog's MongoDB read model is updated this way.

### Multi-tenancy

- `IScoped` entities carry `TenantId`/`Scope` and are isolated via EF Core global query filters.
- **Never share a single `DbContext` instance across tenants** for `IScoped`/`IRingFenced` entities — each `DbContext` instance must be scoped to one tenant.
- `EShop.Shared.Scoping` and `EShop.Shared.Authentication` provide tenant-aware context and per-tenant RSA-signed JWTs (key rotation via `TenantKeyProvider` + Redis cache, fallback to in-memory).

### Cross-service integration

Services communicate via MassTransit/RabbitMQ integration events (e.g., Catalog `VariantCreated`/`ProductDeleted` → Inventory creates/deactivates `InventoryItem`; Order `OrderCreated`/`OrderCancelled`/`OrderCompleted` → Inventory reserves/releases/deducts stock and replies with `StockReserved`/`StockReservationFailed`). Integration event contracts live in `EShop.Shared.Contracts/Services/<Service>/`.

### Key shared libraries (`Shared/src/`)

- `EShop.Shared.DomainTools` — base aggregate/entity interfaces (`IAggregateRoot`, `IScoped`, `IRingFenced`, `IAuditable`), specifications, event-sourcing seedwork (`AggregateStore`, Postgres event/snapshot repositories), `RepositoryBase`, domain exceptions.
- `EShop.Shared.CQRS` — command/query/handler abstractions.
- `EShop.Shared.EventBus` — MassTransit integration event abstractions (`IEventBus`, `IntegrationEvent`, `IdempotentConsumer<T>`).
- `EShop.Shared.ReadModel` / `EShop.Shared.ReadModel.EfCore` — read model abstractions for MongoDB projections.
- `EShop.Shared.Authentication` — JWT + per-tenant RSA key management.
- `EShop.Shared.Cache` — Redis distributed cache.
- `EShop.Shared.JsonApi` — JSON:API controllers/resource access.
- `EShop.Shared.Scoping` — multi-tenant scoping/permissions.
- `EShop.Shared.Diagnostics` — OpenTelemetry instrumentation.
- `EShop.Testing.IntegrationTest` / `EShop.Testing.JsonApiApplication` — shared `TestServer`/integration test infra used by service test projects.

## Conventions

- C# nullable reference types enabled; file-scoped namespaces (`csharp_style_namespace_declarations = file_scoped`); `using` directives outside namespace.
- Primary constructors preferred for DI (e.g., command handlers take dependencies as constructor params).
- Domain errors returned as `Result`/`Result<T>` (not exceptions) from command handlers; domain invariants enforced via `Specification` classes that throw `DomainException` when violated.
- Tests: xUnit + FluentAssertions + Moq/AutoFixture; BDD scenarios via Reqnroll.xUnit (Cucumber expressions) in `*.Tests` projects.

### Anti-patterns to avoid

- Don't modify multiple aggregates in a single command — each command touches one aggregate; coordinate across aggregates via integration events.
- Don't publish commands from domain event subscribers — subscribers are only for logging/publishing integration events.
- Don't change an event's `[EventVersion]` name/number without an `IEventUpgrader` — event-sourced events are persisted and must remain deserializable.
- Don't use a single `DbContext` across tenants for `IScoped`/`IRingFenced` entities.

## OpenSpec workflow

This repo uses OpenSpec (`openspec/`) for spec-driven change proposals: `openspec/changes/` holds in-progress/archived change proposals (`proposal.md`, `tasks.md`, `design.md`, `specs/*/spec.md`); `openspec/specs/` holds the current accepted specs. Slash-command workflows live in `.github/skills/openspec-*` and `.github/prompts/opsx-*`.
