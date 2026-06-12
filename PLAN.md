# Base Template - Living Architecture And Delivery Plan

> Last updated: 2026-06-12
>
> This file is the canonical roadmap for the BaseTemplate solution.
> It defines the architectural guardrails, execution order, and production-hardening specifications.
> Update it at the end of every meaningful work session.

---

## 1. Purpose

BaseTemplate is a reusable enterprise .NET solution template for building production-grade systems quickly, designed with a **Production-First Mindset** from the get-go. 

The solution is structured as a **Logical Modular Monolith**:
- **Single Deployable Unit:** Simplifies deployment, operations, and hosting costs while the domain is evolving.
- **Bounded Contexts:** Clear separation of concerns (IAM, Workforce, Banking, Shared) with separate database schemas and DbContexts to prevent cross-context table joins.
- **Clean Architecture:** Strict inward-pointing dependency directions (Domain & SharedKernel -> Application -> Infrastructure & Persistence -> API & UI Hosts).
- **Vertical Slice Structure:** Cross-layer feature folders to maximize readability and reduce code scatteredness.
- **Client Decoupling:** Both Blazor Web and MAUI Hybrid serve as clean HTTP API clients, ensuring the web host requires zero database connectivity or secrets.

---

## 2. Architectural Direction

### 2.1 Core Architecture
The platform implements:
- Bounded Contexts with context-scoped `DbContext` and `IUnitOfWork`.
- CQRS via MediatR (templated) and Mediator.SourceGenerator (compiled).
- Domain Events (in-process) and Integration Events with MassTransit EF Outbox (cross-process).
- Architecture-tests-as-guardrails to block regression in CI/CD.

```
┌────────────────────────────────────────────────────────┐
│                      BT.UI.Rcl                         │ (Shared Razor Components)
└───────────┬────────────────────────────────┬───────────┘
            │ (HTTP REST)                    │ (HTTP REST)
┌───────────▼───────────┐        ┌───────────▼───────────┐
│     BT.UI.Blazor      │        │      BT.UI.Maui       │ (UI Hosts)
└───────────┬───────────┘        └───────────┬───────────┘
            └────────────────┬───────────────┘
                             │ (JSON over HTTP)
                    ┌────────▼────────┐
                    │     BT.Api      │ (Host Controller Gateway)
                    └────────┬────────┘
        ┌────────────────────┼────────────────────┐
┌───────▼───────┐    ┌───────▼───────┐    ┌───────▼───────┐
│BT.Infrastructure│  │BT.Persistence │    │BT.Application │ (Infrastructure / Data / Logic)
└───────┬───────┘    └───────┬───────┘    └───────┬───────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             ▼
                     ┌───────────────┐
                     │   BT.Domain   │ (Core Entities & Business Rules)
                     └───────────────┘
```

### 2.2 Bounded Context Guardrails
To prevent logical modular monoliths from decaying into a "distributed big ball of mud," the following constraints are enforced:
1. **No Cross-DbContext Queries:** Modules can never join tables belonging to another context in SQL.
2. **References by ID Only:** Domain entities in one context (e.g., Banking `Customer`) must refer to entities in another context (e.g., IAM `AppUser`) using IDs, never navigation properties.
3. **Communication via Events:** Cross-context mutations are synchronized asynchronously using Integration Events and the MassTransit Outbox.
4. **Compile-time Assembly/Namespace Checks:** `NetArchTest` checks in `BT.Tests.Architecture` enforce that code inside a module namespace (e.g., `BT.Application.Features.Banking`) does not reference classes in another module namespace (e.g., `BT.Application.Features.HR`) directly.

### 2.3 Feature Folder Convention
Code is organized horizontally by bounded context and vertically by feature:
```text
src/Backend/Application/BT.Application/Features/{BoundedContext}/{Feature}
```
All query records, command records, validators, mapping profiles, and handlers for a given feature slice reside in the same folder. Shared plumbing (observability, caching abstractions, logging, base controllers) lives in root-level directories.

---

## 3. Current State

### 3.1 Architecture Work Completed
- **Decoupled Frontend:** Blazor Web App (`BT.UI.Blazor`) restructured as a pure API consumer over HTTP (removes database dependencies, secrets, and MediatR references from the Web UI project).
- **Multi-Context Persistence:** Separate DbContexts (`IamDBContext`, `HrDBContext`, `BankingDBContext`, and `SharedDBContext`) created.
- **Shared Kernel & Validation:** Created `BT.SharedKernel` (DTOs) and `BT.SharedKernel.Validation` (Validators) to share validation logic between client-side Blazor/MAUI and backend MediatR behaviors.
- **IAM/Auth Enterprise Baseline:** Auth endpoints expose login, refresh, logout, current-user, permission-based authorization, server-side session tracking, sliding refresh token rotation, MFA enrollment/disable, trusted-device support, inactivity warning/logout, and profile-picture upload through a storage abstraction.
- **Audit Actor Stamping:** Persistence audit fields use the current actor provider where available and fall back to `System` only for background/startup work.
- **Profile Image Storage:** User profile pictures are supported through `IProfilePictureStorage`; local development stores files under API static assets, while Azure Blob can be added behind the same abstraction during cloud hardening.
- **Architecture Tests:** Guardrails built enforcing that all Queries declare cache strategies and all Banking/HR write commands declare cache invalidation.

### 3.2 Areas That Must Still Be Completed
- **HybridCache Stampede Protection Fix:** Refactor `ICacheService` to leverage native `GetOrCreateAsync` factories to enable stampede prevention (request coalescing) and avoid negative cache envelope pollution.
- **Output Caching & Edge Headers:** Set up HTTP Output Cache middleware for static lookup data.
- **ASP.NET Data Protection API Hardening:** Configure Key Vault and Blob Storage key persistence for horizontal scaling.
- **Production Migrations:** Build a pipeline using `efbundle` to execute schema changes safely during deployments instead of using `db.Database.Migrate()` on startup.
- **Dynamic Navigation Maturity:** Dynamic menu/catalog management exists; continue tightening permission filtering, tenant/department scope rules, and client-facing administration workflows as feature modules grow.
- **observability & Health Checks:** Wire deep, operational health checks for SQL, Redis, Service Bus, and Key Vault.
- **Production Smoke Testing:** Run the full local smoke cycle against real SMTP/user-secrets/Key Vault settings before promoting to Azure deployment work.
- **Local Platform:** Wire a comprehensive `docker-compose` environment for SQL Server, Redis, Seq, RabbitMQ, and LocalStorage.

---

## 4. Production Hardening Conventions

For a template to be production-ready, it must enforce operational and security standards by design.

### 4.1 Caching Design System
1. **No Cache-Aside with Dummy Factories:** Always pass data-fetching delegates directly to `HybridCache` to preserve stampede protection:
   ```csharp
   // Standard Query Caching Behavior
   return await cache.GetOrCreateAsync(
       key, 
       async ct => await next(ct), 
       expiration, 
       cancellationToken);
   ```
2. **Synchronized L1 Eviction:** Configure `HybridCache` to connect to Redis as the pub/sub backplane. When an instance removes or updates a key, it must broadcast an eviction message to clear L1 caches on other API nodes.
3. **HTTP Output Caching:** Use `AddOutputCache()` in the Web API layer for static, public endpoints (such as `/api/v1/lookups`). This bypasses controllers, DI, and DB layers completely, serving directly from cache at the routing layer.

### 4.2 Database Operations
1. **Zero Startup Migrations in Production:** Calling `context.Database.Migrate()` on API startup is an anti-pattern. Under load or during scaling, multiple container instances starting up concurrently will attempt to migrate the database simultaneously, causing transaction deadlocks or schema corruption.
   - *Requirement:* Build and publish database migrations via CI/CD using migration bundles (`efbundle`) executed as a release step.
2. **Tenant Isolation:** Enforce tenant scope globally. Every multi-tenant entity must implement `ITenantScoped` with a query filter on the `DbContext` bound to `CurrentTenantId`.

### 4.3 Security & Compliance
1. **Data Protection API Key Storage:** By default, ASP.NET Core stores cryptographic keys (used for cookies, session tokens, and Anti-Forgery tokens) on local disk. In a cloud environment with multiple instances, this causes random decryption errors (invalid session/CSRF tokens) during scaling.
   - *Requirement:* Configure `AddDataProtection()` to store keys in Azure Blob Storage and encrypt them using Azure Key Vault.
2. **Serilog PII Destructuring:** Guard logs against leaking sensitive data (passwords, TOTP secrets, SSNs). Use destructuring policies or ignore rules on logging configurations to strip sensitive payload properties before they hit App Insights or Seq.
3. **CSRF Enforcement:** Explicitly configure Anti-Forgery tokens on the API and Blazor hosts for all state-changing HTTP requests (POST, PUT, DELETE) using cookie authentication.

### 4.4 Observability Event Ranges
EventIds are structured by architectural layer to simplify searching in logs:

| Layer | Range | Purpose |
| --- | --- | --- |
| API | 1000-1999 | Route matching, authentication, controller execution |
| Application | 2000-2999 | CQRS Request pipelines, validation, cache triggers |
| Infrastructure | 3000-3999 | Caching engines, email/SMS gateways, vault secrets |
| Persistence | 4000-4999 | EF Core queries, migrations, Unit of Work |
| Domain | 5000-5999 | Domain events, invariants failures |

---

## 5. Capability Roadmap

```
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE A - Caching & Modular Monolith Hardening           │
  └──────────────────────────┬───────────────────────────────┘
                             ▼
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE B - Operational Observability & Health Checks      │
  └──────────────────────────┬───────────────────────────────┘
                             ▼
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE C - CI/CD Pipelines & Zero-Downtime Deployments    │
  └──────────────────────────┬───────────────────────────────┘
                             ▼
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE D - UI Completion, Dynamic Menus & Payments        │
  └──────────────────────────────────────────────────────────┘
```

### Phase A - Caching & Modular Monolith Hardening
*Goal: Harden cache performance and block module coupling in the logical monolith.*
- [ ] **Harden HybridCacheService:** Refactor `ICacheService` and `CachingBehavior` to use `GetOrCreateAsync` with the handler delegate instead of dummy envelopes, restoring stampede protection.
- [ ] **Enable Redis L1 Eviction Bus:** Configure StackExchange.Redis ConnectionMultiplexer as the backplane for HybridCache L1 invalidation broadcasting.
- [ ] **Add Namespace Dependency Tests:** Introduce namespace checks in NetArchTest to assert that Banking, HR, IAM, and Shared application feature slices cannot cross-reference.
- [ ] **Configure Output Caching:** Register output caching middleware in `BT.Api` and apply caching to lookup controllers.

### Phase B - Operational Observability & Health Checks
*Goal: Ensure the system can be monitored, scaled, and diagnosed under load.*
- [ ] **Deep Health Checks:** Replace basic TCP check endpoints with active health checks testing actual database write, cache set, service bus connection, and Key Vault secret retrieval.
- [ ] **Configure ASP.NET Data Protection:** Register Azure Blob Storage and Key Vault encryption for Data Protection keys.
- [ ] **PII Log Sanitization:** Implement Serilog destructuring policies to mask passwords, MFA codes, and personal identifiers.
- [ ] **Correlated Tracing:** Ensure OpenTelemetry trace context propagates across Blazor client HTTP requests -> API gateways -> MediatR pipelines -> EF Core.

### Phase C - CI/CD Pipelines & Zero-Downtime Deployments
*Goal: Establish secure, repeatable deployment guardrails without manual steps.*
- [ ] **efbundle Migration Pipeline:** Configure GitHub Actions to compile and run EF Core migration bundles against SQL Server during the release pipeline.
- [ ] **Docker Local Platform:** Finalize `docker-compose` to run SQL Server, Redis, RabbitMQ, and Seq in one command.
- [ ] **Azure Deployment Workflows:** Wire Blue/Green deployment slot switches for API services in GitHub Actions.

### Phase D - UI Completion, Dynamic Menus & Payments
*Goal: Complete frontend integration, dynamic navigation, and payments.*
- [ ] **Dynamic Permissions-Based Menus:** Implement backend menu/module API that constructs the UI layout based on user permissions, roles, and active feature flags.
- [ ] **MudBlazor UI Shell:** Implement responsive theme, layouts, and auth pages in the Shared Razor Class Library (RCL).
- [ ] **Payment Gateway Integrations:** Write clean Stripe and Mpesa integrations behind a unified `IPaymentGateway` interface.

---

## 6. Open Decisions

- **Cookie vs. Hybrid Auth in Blazor:** Should Blazor use pure cookie authentication with backend session matching or access tokens (JWT) stored client-side?
  - *Prod Context:* Cookies are more secure against XSS, but require a reverse proxy or same-site routing. JWTs allow client-side independence.
- **Audit Logging Model:** Should we use EF Core interceptors mapping changes to a central `AuditLogs` table, or push audit events to an asynchronous message broker for external archiving?
- **Module Physical Split Trigger:** At what team or codebase size should folders in `BT.Domain` / `BT.Application` be physically split into individual project assemblies?

---

## 7. Session Onboarding

**Start of Session:**
1. Review this document.
2. Ensure Docker containers for SQL, Redis, and Seq are running locally.
3. Run `tests/checks.ps1` or run tests locally via Visual Studio to ensure base health.

**End of Session:**
1. Update this roadmap to check off completed capabilities.
2. Run architecture tests to confirm no structural boundaries were broken.
3. Commit using descriptive, outcome-focused messages.
