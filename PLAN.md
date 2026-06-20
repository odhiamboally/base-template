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

### 3.2 Areas That Must Still Be Completed Or Certified
- **Platform Storage Certification:** Profile image storage now supports local and Azure Blob providers behind `IProfilePictureStorage`; Data Protection supports local keys plus Azure Blob/Key Vault configuration. The remaining gate is local build plus Azure configuration smoke once cloud resources are ready.
- **HybridCache Certification:** Query caching and output caching are wired; complete the remaining review for native `GetOrCreateAsync` usage, negative-cache behavior, and Redis-backed multi-node invalidation.
- **MassTransit/Outbox Certification:** RabbitMQ and Azure Service Bus transports are configurable and MassTransit EF Outbox is registered. The remaining gate is an end-to-end publish/consume smoke with outbox persistence and transport-specific health checks.
- **Exception And Validation Coverage:** ProblemDetails and frontend parsing are in place. The remaining gate is validator coverage review for command/request DTOs and tests proving backend validation messages surface cleanly in Blazor.
- **API Security And Lifecycle:** Security headers, rate limiting, CORS, JWT, permission policies, and API versioning are present. The remaining gate is deprecation policy, throttle response consistency, CSRF stance, and operational tests.
- **Production Migrations:** Build a pipeline using `efbundle` to execute schema changes safely during deployments instead of using `db.Database.Migrate()` on startup.
- **Dynamic Navigation Maturity:** Dynamic menu/catalog management exists; continue tightening permission filtering, tenant/department scope rules, and client-facing administration workflows as feature modules grow.
- **Observability & Health Checks:** Wire deep, operational health checks for SQL, Redis, Service Bus, and Key Vault.
- **Production Smoke Testing:** Run the full local smoke cycle against real SMTP/user-secrets/Key Vault settings before promoting to Azure deployment work.
- **Local Platform:** Wire a comprehensive `docker-compose` environment for SQL Server, Redis, Seq, RabbitMQ, and local storage alternatives.

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
  │ PHASE 1 - IAM/Auth Enterprise Baseline                   │
  └──────────────────────────┬───────────────────────────────┘
                             ▼
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE 2 - Platform Storage, Cache, Validation, Messaging │
  └──────────────────────────┬───────────────────────────────┘
                             ▼
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE 3 - API Security & Operational Readiness           │
  └──────────────────────────┬───────────────────────────────┘
                             ▼
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE 4 - CI/CD Pipelines & Azure Deployment             │
  └──────────────────────────┬───────────────────────────────┘
                             ▼
  ┌──────────────────────────────────────────────────────────┐
  │ PHASE 5 - Template Extensibility                         │
  └──────────────────────────────────────────────────────────┘
```

### Phase 1 - IAM/Auth Enterprise Baseline
*Goal: certify identity, authentication, authorization, MFA, sessions, and local UI/API smoke testing.*
- [~] **IAM/Auth Baseline:** Login, refresh, logout, server-side sessions, lockout, TOTP MFA, admin MFA enforcement, grant/revoke access, current-user, profile picture upload, and inactivity warning are implemented.
- [ ] **Local Smoke Certification:** Complete browser/email smoke testing for login, MFA challenge/setup/disable, grant access email, revoke access, refresh, logout, and session timeout.

### Phase 2 - Platform Storage, Cache, Validation, Messaging
*Goal: harden shared platform services before deployment work.*
- [~] **Harden HybridCacheService:** Query caching is wired; certify `GetOrCreateAsync` behavior and remove any remaining dummy/negative cache paths that can pollute cache state.
- [~] **Enable Redis L1 Eviction Bus:** Redis distributed cache and `IConnectionMultiplexer` registration exist; certify multi-node invalidation behavior explicitly.
- [x] **Add Namespace Dependency Tests:** NetArchTest blocks cross-bounded-context namespace references.
- [x] **Configure Output Caching:** `AddOutputCache`, `UseOutputCache`, and lookup endpoint policies are registered.
- [~] **Profile Media Storage:** Local and Azure Blob profile picture providers exist behind `IProfilePictureStorage`; certify Azure settings with real storage.
- [~] **Validation Coverage:** Shared DTO validators and Application write-command validators are being expanded for admin/customer/employee/reference flows.
- [~] **MassTransit/Outbox:** RabbitMQ/Azure Service Bus switching and EF outbox are wired; certify transport-specific publish/consume and health behavior.

### Phase 3 - API Security And Operational Readiness
*Goal: Ensure the system can be monitored, scaled, and diagnosed under load.*
- [ ] **Deep Health Checks:** Replace basic TCP check endpoints with active health checks testing actual database write, cache set, service bus connection, and Key Vault secret retrieval.
- [~] **Configure ASP.NET Data Protection:** Local file/DPAPI and Azure Blob/Key Vault settings are wired; certify with Azure resources.
- [ ] **PII Log Sanitization:** Implement Serilog destructuring policies to mask passwords, MFA codes, and personal identifiers.
- [ ] **Correlated Tracing:** Ensure OpenTelemetry trace context propagates across Blazor client HTTP requests -> API gateways -> MediatR pipelines -> EF Core.
- [ ] **API Lifecycle:** Define deprecation headers, throttle response consistency, and API security policy tests.

### Phase 4 - CI/CD Pipelines & Azure Deployment
*Goal: Establish secure, repeatable deployment guardrails without manual steps.*
- [~] **efbundle Migration Pipeline:** GitHub Actions compiles context-specific EF migration bundles and executes them through OIDC with Azure SQL wake-up retries and temporary runner firewall access. The remaining gate is the first successful Azure migration/deployment smoke.
- [ ] **Docker Local Platform:** Finalize `docker-compose` to run SQL Server, Redis, RabbitMQ, and Seq in one command.
- [ ] **Azure Deployment Workflows:** Wire Blue/Green deployment slot switches for API services in GitHub Actions.

### Phase 5 - Template Extensibility
*Goal: complete reusable extension points without adding product-specific SACCO/domain features to the template.*
- [ ] **Dynamic Permissions-Based Menus:** Implement backend menu/module API that constructs the UI layout based on user permissions, roles, and active feature flags.
- [ ] **MudBlazor UI Shell:** Implement responsive theme, layouts, and auth pages in the Shared Razor Class Library (RCL).
- [ ] **Feature Flags:** Add a generic feature-flag abstraction usable from API/Application/UI.
- [ ] **SignalR Baseline:** Add authenticated hub structure and conventions.
- [ ] **Reporting Baseline:** Add QuestPDF/reporting abstraction without product-specific reports.
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
