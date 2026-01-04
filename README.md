# 🛒 EShop SaaS Platform

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Microservices-blue)](/)
[![Pattern](https://img.shields.io/badge/Pattern-CQRS%20%2B%20Event%20Sourcing-green)](/)
[![Observability](https://img.shields.io/badge/Observability-OpenTelemetry-orange)](https://opentelemetry.io/)

> **A production-ready multi-tenant e-commerce platform** demonstrating enterprise-grade microservices architecture, domain-driven design, and cloud-native observability practices.

---

## 📋 Table of Contents

- [Executive Summary](#-executive-summary)
- [Architecture Overview](#-architecture-overview)
- [Technology Stack](#-technology-stack)
- [Design Patterns & Principles](#-design-patterns--principles)
- [Project Structure](#-project-structure)
- [Observability](#-observability)
- [Getting Started](#-getting-started)
- [Technical Decisions](#-technical-decisions)

---

## 🎯 Executive Summary

| Aspect | Description |
|--------|-------------|
| **What** | Multi-tenant SaaS e-commerce platform |
| **Architecture** | Microservices with CQRS + Event Sourcing |
| **Key Patterns** | Clean Architecture, DDD, Event-Driven |
| **Infrastructure** | .NET Aspire, Docker, PostgreSQL, MongoDB, Redis, RabbitMQ |
| **Observability** | OpenTelemetry → Prometheus → Grafana |

### 💡 Skills Demonstrated

```
✅ Microservices Design          ✅ Domain-Driven Design         ✅ Event Sourcing & CQRS
✅ Distributed Systems           ✅ Multi-tenancy                ✅ Cloud-Native Patterns
✅ Observability (Metrics/Traces/Logs)                           ✅ Clean Architecture
```

---

## 🏗 Architecture Overview

### High-Level System Design

```
                                 ┌───────────────────────┐
                                 │        CLIENTS        │
                                 │  Web │ Mobile │ API   │
                                 └───────────┬───────────┘
                                             │
                              ┌──────────────▼──────────────┐
                              │     API GATEWAY / PROXY     │
                              └──────────────┬──────────────┘
                                             │
┌────────────────────────────────────────────┼────────────────────────────────────────────┐
│                                    MICROSERVICES                                        │
│                                                                                         │
│    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐             │
│    │   TENANCY   │    │    AUTH     │    │   CATALOG   │    │  IDENTITY   │             │
│    │             │    │             │    │             │    │             │             │
│    │  • Tenants  │    │  • Users    │    │  • Products │    │  • Login    │             │
│    │  • Settings │    │  • Roles    │    │  • Stock    │    │  • Tokens   │             │
│    │  • Features │    │  • Perms    │    │  • Category │    │  • SSO      │             │
│    └──────┬──────┘    └──────┬──────┘    └──────┬──────┘    └──────┬──────┘             │
│           │                  │                  │                  │                    │
└───────────┼──────────────────┼──────────────────┼──────────────────┼────────────────────┘
            │                  │                  │                  │
┌───────────┼──────────────────┼──────────────────┼──────────────────┼────────────────────┐
│           │                  │     INFRASTRUCTURE                  │                    │
│           ▼                  ▼                  ▼                  ▼                    │
│    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐             │
│    │ PostgreSQL  │    │    Redis    │    │   MongoDB   │    │  RabbitMQ   │             │
│    │   Events    │    │    Cache    │    │ Read Models │    │  Messaging  │             │
│    └─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘             │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### Data Flow (CQRS + Event Sourcing)

```
┌─────────┐      ┌─────────┐      ┌───────────┐      ┌─────────────┐
│ REQUEST │ ───► │   API   │ ───► │  COMMAND  │ ───► │  AGGREGATE  │
└─────────┘      └─────────┘      │    BUS    │      │    ROOT     │
                                  └───────────┘      └──────┬──────┘
                                                            │
                                                    Domain Events
                                                            │
                 ┌──────────────────────────────────────────┤
                 │                                          │
                 ▼                                          ▼
          ┌─────────────┐                           ┌─────────────┐
          │ EVENT STORE │                           │ SUBSCRIBERS │
          │ (PostgreSQL)│                           └──────┬──────┘
          └─────────────┘                                  │
                                          ┌────────────────┼────────────────┐
                                          ▼                                 ▼
                                   ┌─────────────┐                   ┌─────────────┐
                                   │ READ MODEL  │                   │ INTEGRATION │
                                   │  (MongoDB)  │                   │   EVENTS    │
                                   └──────┬──────┘                   └──────┬──────┘
                                          │                                 │
                                          ▼                                 ▼
                                   ┌─────────────┐                   ┌─────────────┐
                                   │   QUERIES   │                   │   OTHER     │
                                   │  Response   │                   │  SERVICES   │
                                   └─────────────┘                   └─────────────┘
```

---

## 🛠 Technology Stack

### Core Technologies

| Category | Technology | Version | Purpose |
|:---------|:-----------|:--------|:--------|
| **Platform** | .NET | 10.0 | Runtime framework |
| **Orchestration** | .NET Aspire | 9.x | Service orchestration & local dev |
| **API** | ASP.NET Core | 10.0 | Web API framework |
| **Specification** | JSON:API | - | RESTful API standard |

### Architecture & Patterns

| Category | Technology | Purpose |
|:---------|:-----------|:--------|
| **CQRS/ES** | EventFlow | Command/Query separation, Event Sourcing |
| **Messaging** | MassTransit + RabbitMQ | Async communication, Integration events |
| **Background Jobs** | Hangfire | Scheduled & background processing |

### Data & Cache

| Category | Technology | Purpose |
|:---------|:-----------|:--------|
| **Event Store** | PostgreSQL | ACID-compliant event persistence |
| **Read Models** | MongoDB | Optimized query storage |
| **Cache** | Redis | Distributed caching |

### Observability

| Category | Technology | Purpose |
|:---------|:-----------|:--------|
| **Instrumentation** | OpenTelemetry | Vendor-neutral telemetry |
| **Metrics** | Prometheus | Time-series metrics storage |
| **Visualization** | Grafana | Dashboards & alerting |
| **Traces/Logs** | Aspire Dashboard | Distributed tracing & logs |

### Testing

| Category | Technology | Purpose |
|:---------|:-----------|:--------|
| **Unit Testing** | xUnit | Test framework |
| **Mocking** | FakeItEasy | Test doubles |
| **BDD** | Reqnroll | Behavior-driven development |

---

## 📐 Design Patterns & Principles

### Architecture Patterns

| Pattern | Implementation | Benefit |
|:--------|:---------------|:--------|
| **Clean Architecture** | Domain → Application → Infrastructure → API | Testability, maintainability |
| **CQRS** | Separate Command/Query models | Optimized read/write paths |
| **Event Sourcing** | Immutable event stream | Full audit trail, temporal queries |
| **Microservices** | Bounded context per service | Independent deployment |

### Domain-Driven Design

| Concept | Description |
|:--------|:------------|
| **Aggregates** | Consistency boundaries (Tenant, User, Product) |
| **Domain Events** | Immutable facts representing state changes |
| **Specifications** | Encapsulated, reusable business rules |
| **Value Objects** | Immutable domain primitives |

### Cross-Cutting Concerns

| Concern | Implementation |
|:--------|:---------------|
| **🔐 Multi-tenancy** | Request-scoped tenant isolation |
| **🔑 Authentication** | JWT tokens, policy-based authorization |
| **📝 Logging** | Structured logs with correlation IDs |
| **🔍 Tracing** | Distributed tracing across services |
| **⚠️ Exception Handling** | Global middleware, domain exceptions |
| **✅ Validation** | FluentValidation, domain specifications |
| **💾 Caching** | Redis distributed cache |

---

## 📁 Project Structure

```
EShop/
│
├── 🚀 EShop.AppHost/                  # .NET Aspire orchestration
├── 📦 EShop.ServiceDefaults/          # Shared OpenTelemetry & health checks
│
├── 📂 Tenancy/                        # ── Tenant Management Context ──
│   ├── src/
│   │   ├── EShop.Tenancy.API/         #    API Layer
│   │   ├── EShop.Tenancy.Application/ #    Application Layer (CQRS)
│   │   ├── EShop.Tenancy.Domain/      #    Domain Layer (Aggregates)
│   │   └── EShop.Tenancy.Infrastructure/ # Infrastructure Layer
│   └── tests/
│       └── EShop.Tenancy.Tests/       #    Unit & BDD Tests
│
├── 📂 Authorization/                  # ── User & Permission Context ──
├── 📂 Catalog/                        # ── Product Catalog Context ──
├── 📂 Identity/                       # ── Identity Context ──
│
├── 📂 Shared/                         # ── Cross-Cutting Libraries ──
│   └── src/
│       └── EShop.Shared.Diagnostics/  #    Logging & Telemetry
│
└── 📂 Deployment/
    └── config/
        ├── otelcollector/             #    OpenTelemetry Collector
        ├── prometheus/                #    Prometheus configuration
        └── grafana/                   #    Grafana dashboards
```

---

## 📊 Observability

### Telemetry Pipeline

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            OBSERVABILITY STACK                              │
│                                                                             │
│    ┌───────────┐    ┌───────────┐    ┌───────────┐                          │
│    │  Tenancy  │    │   Auth    │    │  Catalog  │       Services           │
│    └─────┬─────┘    └─────┬─────┘    └─────┬─────┘                          │
│          │                │                │                                │
│          └────────────────┼────────────────┘                                │
│                           │ OTLP                                            │
│                           ▼                                                 │
│               ┌───────────────────────┐                                     │
│               │    OTEL COLLECTOR     │          Telemetry Gateway          │
│               └───────────┬───────────┘                                     │
│                           │                                                 │
│          ┌────────────────┼────────────────┐                                │
│          ▼                ▼                ▼                                │
│    ┌───────────┐    ┌───────────┐    ┌───────────┐                          │
│    │  ASPIRE   │    │PROMETHEUS │    │  GRAFANA  │       Backends           │
│    │ DASHBOARD │    │           │    │           │                          │
│    └───────────┘    └───────────┘    └───────────┘                          │
│     Traces/Logs        Metrics        Dashboards                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Signals & Backends

| Signal | Backend | Metrics Captured |
|:-------|:--------|:-----------------|
| **📈 Metrics** | Prometheus → Grafana | Request latency, error rates, throughput, connections |
| **🔗 Traces** | Aspire Dashboard | Distributed request flow, span timing |
| **📝 Logs** | Aspire Dashboard | Structured logs with correlation |

---

## 🚀 Getting Started

### Prerequisites

```bash
# Required
✅ .NET 10 SDK
✅ Docker Desktop
```

### Quick Start

```bash
# Clone & Run
git clone https://github.com/mnnam1302/EShop.git
cd EShop/EShop.AppHost
dotnet run
```

### Access Points

| Service | Description |
|:--------|:------------|
| **Aspire Dashboard** | Resource management, traces, logs |
| **Grafana** | Metrics dashboards |
| **API Endpoints** | See Aspire Dashboard for dynamic URLs |

---

## 🧠 Technical Decisions

| Decision | Rationale |
|:---------|:----------|
| **Event Sourcing** | Complete audit trail, temporal queries, event replay capability |
| **CQRS** | Independent optimization of read/write models |
| **PostgreSQL (Events)** | ACID compliance critical for event store integrity |
| **MongoDB (Read Models)** | Flexible schema for query-optimized projections |
| **.NET Aspire** | Simplified orchestration, built-in observability, developer productivity |
| **OpenTelemetry** | Vendor-neutral observability, industry standard |
| **RabbitMQ + MassTransit** | Reliable messaging with saga support |

---

## 📄 License

This project is for educational and portfolio demonstration purposes.

---

<div align="center">

**Built with ❤️ using .NET**

[![GitHub](https://img.shields.io/badge/GitHub-mnnam1302-181717?logo=github)](https://github.com/mnnam1302)

</div>


