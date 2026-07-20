# Tenancy

> Owns **who the tenants are** — provisioning a tenant, the system-wide **feature catalog** and per-tenant feature flags, per-tenant **settings**, and the per-tenant **rate-limit policy** that the platform's distributed rate limiter enforces. This is the context every other service trusts to answer "does tenant X exist, and what is it allowed to do?".

---

## What This Service Does

```mermaid
graph LR
    classDef actor   fill:#fff176,stroke:#f9a825,color:#000
    classDef agg     fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef svc     fill:#a5d6a7,stroke:#2e7d32,color:#000
    classDef infra   fill:#b0bec5,stroke:#455a64,color:#000

    SUP([Support User]):::actor
    SYS([System User]):::actor
    OTHER([Other Services]):::svc
    TEN(Tenancy Service):::agg
    RL([Distributed Rate Limiter]):::svc
    CACHE[(Redis feature cache)]:::infra

    SUP -->|"POST /tenants · PUT /rate-limit-policy"| TEN
    SYS -->|"enable feature · create feature · queries"| TEN
    OTHER -->|"SupportedFeaturesUpdated (register features)"| TEN
    TEN -->|"TenantCreated"| OTHER
    TEN -->|"TenantFeaturesUpdated → invalidate"| CACHE
    TEN -->|"rate-limit policy (read on demand)"| RL
```

---

## Strategic Design

### Context Classification

| Aspect | Value |
|--------|-------|
| **Bounded Context** | Tenancy |
| **Domain Type** | Supporting Domain |
| **Aggregate Roots** | `Tenant` (owns `TenantFeature`, `TenantSetting`), `Feature` (system feature catalog) |
| **Multi-tenancy** | `Tenant` & `Feature` are `IExcludedFromScoping` (they *define* tenants, so cannot be tenant-scoped); `TenantFeature` & `TenantSetting` are `IScoped`. The service writes scoped rows under an explicit **system-user scope** (`CreateSystemUserScope(tenantId)`). |
| **Persistence** | EF Core (PostgreSQL) |
| **Read Model** | None (Redis cache for the hot per-tenant feature set) |
| **Architecture Style** | Clean Architecture (Domain · Application · Infrastructure · Persistence · Presentation) |

### Bounded Context Map

```mermaid
graph TB
    classDef ctx fill:#eaf2f8,stroke:#1a5276,color:#000

    subgraph TenCtx["Tenancy Context (Supporting)"]
        TEN[Tenant · Feature · TenantSetting]
    end
    subgraph AuthCtx["Authorization Context"]
        AUTH[Organization · User]
    end
    subgraph AnySvc["Any Service (Catalog, Order, …)"]
        SVC[Feature-owning modules]
    end
    subgraph Platform["Shared Platform"]
        RL[Distributed Rate Limiter]
    end

    TEN -->|"TenantCreated<br/>(Customer–Supplier)"| AUTH
    SVC -->|"SupportedFeaturesUpdated<br/>(register capabilities)"| TEN
    TEN -->|"rate-limit policy<br/>(Conformist read)"| RL

    class TenCtx,AuthCtx,AnySvc,Platform ctx
```

> Tenancy is **upstream** of the whole platform: it is the source of truth for tenant existence and entitlements. It never depends on downstream services — it only announces facts (`TenantCreated`) and absorbs feature registrations.

> **Why this service has no "Participants & Roles" table.** Per the documentation standard, Event-Storming *Participants & Roles* is reserved for **Core Domain** services. Tenancy is a **Supporting** domain, so that subsection is intentionally omitted here while every other section stays consistent with the Core services.

### Ubiquitous Language

| Term | Definition |
|------|------------|
| **Tenant** | An isolated customer of the platform. Identified by a lowercased string id (`acme`); owns its features and settings. |
| **System tenant** | A bootstrap tenant for platform administration, created on startup by `SystemInitializer`. |
| **Feature** | A capability in the *system-wide* catalog (`Id`, `Module`, `State`, `DefaultStateForNewTenant`). Owned by whichever module registers it. |
| **TenantFeature** | The per-tenant enablement of a catalog `Feature` — its `State` is `Enabled` / `Disabled`. |
| **Feature registration** | A service announcing the features it owns via `SupportedFeaturesUpdated`; Tenancy upserts them into the catalog. |
| **TenantSetting** | Per-tenant formatting defaults (date/time/currency/language) plus the tenant's `RateLimitPolicy`. |
| **RateLimitPolicy** | A set of `RateLimitRule`s (`Domain`, `Scope`, `Unit`, `RequestsPerUnit`, `Burst`) — the config the distributed rate limiter reads to throttle a tenant. |
| **Support user / System user** | Elevated principals: only a *support* user may create a tenant or set its policy; only a *system* user may manage features. |

---

## Event Storming

### Legend

```mermaid
graph LR
    A["👤 Actor"]:::actor -- issues --> C["Command"]:::command
    C -- handled by --> AGG["Aggregate"]:::aggregate
    AGG -- emits --> E["Integration Event"]:::event
    E -- triggers --> P["Policy"]:::policy
    E -- projects to --> R["Cache / Read"]:::readmodel

    classDef actor    fill:#fff176,stroke:#f9a825,color:#000
    classDef command  fill:#42a5f5,stroke:#1565c0,color:#fff
    classDef aggregate fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef event    fill:#ff9800,stroke:#e65100,color:#fff
    classDef policy   fill:#ce93d8,stroke:#7b1fa2,color:#000
    classDef readmodel fill:#a5d6a7,stroke:#2e7d32,color:#000
```

### Actors

```mermaid
graph TB
    subgraph Actors["👤 Actors"]
        direction LR
        SUP["Support User<br/>provisions tenants · sets rate-limit policy"]
        SYS["System User<br/>manages the feature catalog & tenant flags"]
        SVC["Other Services<br/>register the features they own"]
    end
```

| Actor | Interacts With | Example Scenario |
|-------|----------------|------------------|
| **Support User** | `Tenant` aggregate | *As support, I provision a new tenant so a customer can start using the platform.* |
| **System User** | `Feature` / `TenantFeature` | *As the system, I enable a feature for a tenant so their users gain the capability.* |
| **Other Services** | `Feature` catalog | *As Catalog/Order, I register the features I own so tenants can toggle them.* |

### Tenant Aggregate — Event Flow

```mermaid
graph LR
    SUP([Support User]):::actor --> Create[CreateTenantCommand]:::command --> T(Tenant):::aggregate
    T --> TC([TenantCreated]):::event --> DOWN([Authorization & others]):::policy
    SYS([System User]):::actor --> Enable[EnableTenantFeatureCommand]:::command --> T
    T --> TFU([TenantFeaturesUpdated]):::event --> CACHE([Invalidate feature cache]):::readmodel
    SUP --> SetRL[SetTenantRateLimitPolicyCommand]:::command --> T

    classDef actor    fill:#fff176,stroke:#f9a825,color:#000
    classDef command  fill:#42a5f5,stroke:#1565c0,color:#fff
    classDef aggregate fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef event    fill:#ff9800,stroke:#e65100,color:#fff
    classDef policy   fill:#ce93d8,stroke:#7b1fa2,color:#000
    classDef readmodel fill:#a5d6a7,stroke:#2e7d32,color:#000
```

> On `CreateTenant`, the aggregate seeds a default `TenantSetting` and one `TenantFeature` per catalog `Feature` (using each feature's `DefaultStateForNewTenant`), then publishes `TenantCreated`.

### Feature Catalog — Event Flow

```mermaid
graph LR
    SVC([Any Service]):::actor --> SFU([SupportedFeaturesUpdated]):::event --> C[SupportedFeaturesUpdatedConsumer]:::policy
    C --> Upd[UpdateSupportedFeaturesCommand]:::command --> F(Feature catalog):::aggregate
    SYS([System User]):::actor --> CreateF[CreateFeatureCommand]:::command --> F

    classDef actor    fill:#fff176,stroke:#f9a825,color:#000
    classDef command  fill:#42a5f5,stroke:#1565c0,color:#fff
    classDef aggregate fill:#f8bbd0,stroke:#c2185b,color:#000
    classDef event    fill:#ff9800,stroke:#e65100,color:#fff
    classDef policy   fill:#ce93d8,stroke:#7b1fa2,color:#000
```

### Policies — When / Then Rules

| When this event | Then issue this command | Rail / Transport |
|-----------------|-------------------------|------------------|
| `SupportedFeaturesUpdated` (any service registers features) | `UpdateSupportedFeaturesCommand` → upsert/delete `Feature` catalog | MassTransit · idempotent consumer (inbox) |
| `TenantFeaturesUpdated` (a tenant flag changed) | `ClearTenantFeaturesCommand` → invalidate the tenant's Redis feature cache | MassTransit · idempotent consumer (inbox) |
| `TenantCreated` | *(downstream)* Authorization creates the root organization & owner user | MassTransit |

---

## Domain Model

### Aggregate Structure

```mermaid
classDiagram
    class Tenant {
        <<Aggregate Root>>
        string Id
        string Name
        string OwnerUsername
        string Email
        +Create(command)
        +AddTenantFeature(featureId, state, by)
        +AddDefaultTenantSetting()
        +SetRateLimitPolicy(policy)
    }
    class TenantFeature {
        <<Entity>>
        Guid Id
        string TenantId
        string FeatureId
        string State
        +Enable()
        +IsEnabled()
    }
    class TenantSetting {
        <<Entity>>
        Guid Id
        string DisplayDateFormat
        string DefaultCurrency
        string DefaultSystemLanguage
        RateLimitPolicy RateLimitPolicy
    }
    class RateLimitPolicy {
        <<Value Object>>
        IReadOnlyCollection~RateLimitRule~ Rules
    }
    class RateLimitRule {
        <<Value Object>>
        string Domain
        string Scope
        string Unit
        int RequestsPerUnit
        int? Burst
    }
    class Feature {
        <<Aggregate Root>>
        string Id
        string Module
        string State
        string DefaultStateForNewTenant
    }

    Tenant "1" --> "*" TenantFeature : TenantFeatures
    Tenant "1" --> "1" TenantSetting : TenantSettings
    TenantSetting "1" --> "0..1" RateLimitPolicy : RateLimitPolicy
    RateLimitPolicy "1" --> "*" RateLimitRule : Rules
    TenantFeature ..> Feature : references by FeatureId
```

### Building Blocks

| Building Block | Type | Identity | Rationale |
|----------------|------|----------|-----------|
| `Tenant` | **Aggregate Root** | `string Id` (lowercased) | Consistency boundary for a customer; owns its features and settings. `IExcludedFromScoping` — it defines a tenant, so it can't be tenant-filtered. |
| `TenantFeature` | **Entity** (child of `Tenant`) | `Guid Id` | Per-tenant enablement of a catalog feature; `IScoped`. |
| `TenantSetting` | **Entity** (child of `Tenant`) | `Guid Id` | Per-tenant formats + rate-limit policy; `IScoped`. |
| `RateLimitPolicy` / `RateLimitRule` | **Value Object** | By value (no id) | Immutable throttling rules; replaced wholesale via `SetRateLimitPolicy`. |
| `Feature` | **Aggregate Root** (reference catalog) | `string Id` | The system-wide feature registry; `IExcludedFromScoping`. |
| `StateFeature` / `RateLimitScope` / `RateLimitUnit` | **Enumeration** | Enum value | Feature toggle state; rate-limit rule scope (`Tenant`/`User`/`AnonymousIp`) and unit (`Second`/`Minute`/`Hour`/`Day`). |

---

## State Machines

Tenancy has **no Stateless state machine** — its only lifecycle is the feature flag's `Enabled` / `Disabled` toggle, enforced by explicit guards in `TenantFeature` rather than a transition table.

```mermaid
stateDiagram-v2
    [*] --> Disabled : AddTenantFeature (DefaultStateForNewTenant)
    Disabled --> Enabled : Enable() — guarded: must exist, must not be already Enabled
    Enabled --> Enabled : Enable() ❌ rejected (BadRequestException)
```

> `EnableTenantFeatureCommandHandler` guards the transition: it throws `BadRequestException` if the feature is not found for the tenant, or is already enabled — then publishes `TenantFeaturesUpdated`.

---

## Specifications & Invariants

### Specification Map

```mermaid
graph TB
    subgraph TenantSpecs["Tenant / Policy Specs"]
        S1["RateLimitPolicySpecification<br/>━━━━━━━<br/>• ≤ 20 rules<br/>• Scope ∈ Tenant|User|AnonymousIp<br/>• Unit ∈ Second|Minute|Hour|Day<br/>• RequestsPerUnit > 0<br/>• Burst ≥ RequestsPerUnit<br/>• no duplicate (Domain, Scope)"]
    end
```

Tenant creation additionally enforces id/username rules inline (`AssertTenant`): tenant id is lowercased and limited to `a–z 0–9 - _`, reserved ids (support group) are rejected, and the owner username is normalised to `user@tenantId` and stripped of special characters.

### Invariant Enforcement Flow

```mermaid
sequenceDiagram
    participant Handler as SetRateLimitPolicyHandler
    participant Tenant as Tenant
    participant Spec as RateLimitPolicySpecification

    Handler->>Tenant: SetRateLimitPolicy(policy)
    Tenant->>Spec: ThrowDomainErrorIfNotSatisfied(policy)

    alt Specification Satisfied
        Spec-->>Tenant: Pass
        Tenant->>Tenant: assign policy to the single TenantSetting
        Tenant-->>Handler: Success
    else Specification Violated
        Spec-->>Tenant: Throw DomainException
        Note over Tenant: ❌ "rule '…' has an invalid scope / non-positive requestsPerUnit / duplicate …"
    end
```

---

## Architecture

### Layer Overview

```mermaid
flowchart TB
    subgraph Presentation["🌐 Presentation (Carter modules)"]
        EP["TenantApi · FeatureApi (Minimal APIs)"]
    end
    subgraph Application["⚙️ Application Layer"]
        CMD["CreateTenant · EnableTenantFeature · SetRateLimitPolicy · CreateFeature · UpdateSupportedFeatures"]
        QRY["GetTenantDetails · GetTenantFeatures · GetFeatureById · GetRateLimitPolicy"]
    end
    subgraph Domain["🧠 Domain Layer"]
        AGG["Tenant · Feature aggregates"]
        SPEC["RateLimitPolicySpecification"]
    end
    subgraph Infrastructure["🗄 Infrastructure / Persistence"]
        REPO["Repositories · UnitOfWork"]
        CON["Idempotent consumers (inbox)"]
        PROD["TenantFeatureRegistrationService (startup)"]
        CACHE["ITenantFeaturesCachingService (Redis)"]
        DB["EF Core · PostgreSQL"]
    end

    EP --> CMD & QRY
    CMD --> AGG
    AGG --> SPEC
    CMD --> REPO
    REPO --> DB
    CON --> CMD
    QRY --> CACHE
    PROD --> CON
```

### Happy Path — Provision a Tenant

```mermaid
sequenceDiagram
    autonumber
    participant SUP as Support User
    participant API as TenantApi
    participant H   as CreateTenantHandler
    participant T   as Tenant
    participant BUS as IEventBus (RabbitMQ)
    participant AUTH as Authorization

    SUP->>API: POST /api/v1/tenants
    API->>H: CreateTenantCommand
    H->>T: Tenant.Create + AddDefaultTenantSetting
    H->>T: AddTenantFeature ×N (from Feature catalog defaults)
    H->>H: save (system-user scope)
    H->>BUS: publish TenantCreated
    API-->>SUP: 201 Created
    BUS-->>AUTH: TenantCreated → bootstrap root org + owner user
```

### Feature Flag — Cache-Aside + Event Invalidation

```mermaid
sequenceDiagram
    autonumber
    participant SYS as System User
    participant H   as EnableTenantFeatureHandler
    participant BUS as RabbitMQ
    participant C   as TenantFeaturesUpdatedConsumer
    participant CACHE as Redis feature cache
    participant P   as OwnerTenantFeaturesProvider

    SYS->>H: PATCH …/features/{id}/enable
    H->>H: TenantFeature.Enable() (guarded) + save
    H->>BUS: publish TenantFeaturesUpdated
    BUS->>C: consume (idempotent / inbox)
    C->>CACHE: RemoveTenantFeatures(tenantId)
    Note over P: next read misses cache →<br/>recompute enabled features from DB → re-cache
```

---

## Integration Events

| Direction | Contract | Meaning |
|-----------|----------|---------|
| **Out** | `TenantCreated` | A tenant (or the system tenant) was provisioned — consumed by Authorization to create the root organization & owner user. |
| **Out** | `TenantFeaturesUpdated` | A tenant's feature set changed — consumed in-service to invalidate the Redis feature cache. |
| **Out** | `SupportedFeaturesUpdated` | Tenancy registers the features it owns at startup (`TenantFeatureRegistrationService`). |
| **In** | `SupportedFeaturesUpdated` | Any service announces the features it owns → upsert/delete rows in the `Feature` catalog. |
| **In** | `TenantFeaturesUpdated` | Its own event → clear the tenant's cached feature set. |

Contracts live in `Shared/src/EShop.Shared.Contracts/Services/Tenancy/`. All Tenancy events derive from `TenancyEvent` (`[ExcludeFromTopology]`).

---

## Data Model

| Table | One row per | Key constraint |
|-------|------------|----------------|
| `Tenants` | tenant | PK `Id` (string, lowercased); UNIQUE `Name` |
| `TenantFeatures` | feature enablement × tenant | PK `Id` (Guid); FK `TenantId`; carries `FeatureId`, `State`; `IScoped` |
| `TenantSettings` | settings × tenant | PK `Id` (Guid); FK `TenantId` (cascade); `RateLimitPolicy` persisted with the setting; `IScoped` |
| `Features` | catalog feature | PK `Id` (string); `IExcludedFromScoping` |
| `InboxMessages` | processed message id | Deduplication for idempotent consumers (inbox pattern) |

---

## API

| Method | Path | Response | Note |
|--------|------|----------|------|
| `POST` | `/api/v1/tenants` | `201 Created` | Provision a tenant. **Support user** only. |
| `GET` | `/api/v1/tenants/{tenantId}` | `200 OK` | Tenant details. **System user** only. |
| `PATCH` | `/api/v1/tenants/{tenantId}/features/{featureId}/enable` | `204 No Content` | Enable a feature for a tenant. **System user** only. |
| `PUT` | `/api/v1/tenants/{tenantId}/rate-limit-policy` | `204 No Content` | Replace the tenant's rate-limit policy. **Support user** only. |
| `GET` | `/api/v1/tenants/{tenantId}/rate-limit-policy` | `200 OK` | Read the tenant's rate-limit policy. **System user** only. |
| `GET` | `/api/v1/features?tenantId=` | `200 OK` | A tenant's enabled features. **System user** only. |
| `GET` | `/api/v1/features/{featureId}` | `200 OK` | A catalog feature. **System user** only. |
| `POST` | `/api/v1/features` | `201 Created` | Create a system feature. **System user** only. |

---

## Configuration

| Key | Source | Purpose |
|-----|--------|---------|
| `ConnectionStrings:tenancyDatabase` / `DefaultConnection` | Aspire / appsettings | PostgreSQL connection |
| `MasstransitConfiguration` / `rabbitmq` | appsettings | RabbitMQ connection |
| `SystemUser:Email` | appsettings | Email for the bootstrap system tenant owner |

On startup, `SystemInitializer` provisions the system tenant (with a default rate-limit policy — `authorization` domain, `AnonymousIp`, 5/minute) and `TenantFeatureRegistrationService` registers Tenancy's own features.

---

## Tests

`Tenancy/tests/EShop.Tenancy.Tests` (xUnit + Reqnroll BDD + Testcontainers) — feature-file scenarios:

- **Tenant provisioning** — `TenantCreation.feature`
- **Feature catalog** — `CreateSystemFeature.feature`
- **Tenant feature flags** — `EnableTenantFeature.feature`, `GetFeatures`
- **Authorization filters** — support/system-user access rules
- **Architecture** — layer dependency-rule compliance (`ArchitectureTests`)

```bash
dotnet test Tenancy/tests/EShop.Tenancy.Tests
```

---

## Roadmap

### Gap Analysis

| # | Gap | Status |
|---|-----|--------|
| G1 | **No event on rate-limit policy change.** `SetRateLimitPolicy` persists the new policy but emits no integration event; the distributed rate limiter picks it up on its next read / cache expiry rather than being actively notified. | Open |
| G2 | **Feature flag is a one-way toggle in the API.** `TenantFeature.Enable()` exists but there is no `Disable` endpoint — disabling currently only happens via the default state / catalog delete path. | Open |

---

## References

| Resource | Description |
|----------|-------------|
| [Distributed Rate Limiter README](../../../Shared/src/EShop.Shared.RateLimiting/README.md) | Consumer of the per-tenant `RateLimitPolicy` this service owns |
| [Authorization Service README](../../../Authorization/src/EShop.Authorization.API/README.md) | Downstream consumer of `TenantCreated` |
| [Domain-Driven Design](https://www.domainlanguage.com/ddd/) | Eric Evans — Original DDD book |
| [Event Storming](https://www.eventstorming.com/) | Alberto Brandolini — Discovery technique |
