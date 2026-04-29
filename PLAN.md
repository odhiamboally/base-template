# Base Template — Living Project Plan

> **Last updated:** 2026-04-22  
> This document is the canonical reference for the LlanCore Base Template solution.  
> Update it at the end of every significant work session.

---

## What This Project Is

A production-ready, multi-tenant .NET clean-architecture solution serving as a reusable foundation for LlanCore products. It covers:

- Authentication and authorisation (ASP.NET Core Identity + JWT + 2FA/TOTP)
- Customer (Banking domain) and Employee (HR domain) management
- Domain-driven module extension pattern
- CQRS via MediatR with caching, validation, and exception-handling pipeline behaviours
- Integration event publishing and consumption via MassTransit
- Background job scheduling via Hangfire
- Blazor web front-end and .NET MAUI mobile app
- Structured logging (Serilog), health checks, OpenAPI (Scalar)

---

## Architecture at a Glance

```
src/Backend
├── BT.Domain          ← Entities, value objects, domain events, repository/UoW interfaces
├── BT.Application     ← CQRS commands/queries + handlers, pipeline behaviours, contracts
├── BT.Infrastructure  ← Identity (IAM), Banking, HR modules, Messaging, Jobs
├── BT.Persistence     ← EF Core DbContexts, repository + UoW implementations, seeds
└── BT.Api             ← ASP.NET Core Web API (controllers, middleware, Program.cs)

src/Frontend
├── BT.UI.Blazor       ← Web app (uses ISender directly)
└── BT.UI.Maui         ← Mobile app (calls via API endpoints only)

src/Shared
├── BT.SharedKernel             ← DTOs, enums, common configurations
└── BT.SharedKernel.Validation  ← FluentValidation rule sets

tests/
├── BT.Tests.Architecture  ← NetArchTest layer + naming enforcement
├── BT.Tests.Unit
└── BT.Tests.Integration
```

Dependency direction (inward only): `Api → Infrastructure/Persistence → Application → Domain`

---

## Guiding Principles

| Principle | Rule |
|-----------|------|
| **Clean Architecture** | Layers depend inward only; violations fail CI architecture tests |
| **CQRS** | Command record + handler class in the same `.cs` file |
| **No façade services** | Web consumers call `ISender` directly; `AuthService`-style wrappers are retired |
| **Mobile via API** | MAUI app never references Application/Domain directly |
| **Transactions** | Always `ExecuteInTransactionWithRetryAsync`; no manual begin/rollback |
| **Logging** | Source-generated `[LoggerMessage]` only; strict EventId ranges by layer |
| **Secrets** | Azure Key Vault double-hyphen convention: `Section--Setting` |
| **Naming** | Service prefix `LlanCore.BaseTemplate.*` — never `Onboarding.*` |

---

## LoggerMessage EventId Ranges

| Layer | Range |
|-------|-------|
| API (`BT.Api`) | 1000–1999 |
| Application (`BT.Application`) | 2000–2999 |
| Infrastructure (`BT.Infrastructure`) | 3000–3999 |
| Persistence (`BT.Persistence`) | 4000–4999 |
| Domain (`BT.Domain`) | 5000–5999 _(reserved)_ |

---

## Phase Tracker

### ✅ Phase 1 — Foundation
- [x] Solution structure: 5 backend layers + 2 frontend + shared kernel
- [x] Clean Architecture enforced via `BT.Tests.Architecture`
- [x] Directory.Build.props, Global.json, EditorConfig, nuget.config
- [x] .NET MAUI project scaffolding

### ✅ Phase 2 — Domain Layer
- [x] Entities: `AppUser`, `Customer`, `Employee`, ...
- [x] Value objects: `Address`, `CorporateDetail`, `CommunicationPreference`, ...
- [x] Domain events: `CustomerCreatedDomainEvent`, ...
- [x] Repository interfaces: `ICustomerRepository`, `IEmployeeRepository`, `IUserRepository`, ...
- [x] UoW interfaces: `IBankingUnitOfWork`, `IHrUnitOfWork`, `IIamUnitOfWork`, `ISharedUnitOfWork`
- [x] `ITransactionalUnitOfWork` with `ExecuteInTransactionWithRetryAsync`

### ✅ Phase 3 — Application Layer
- [x] MediatR pipeline behaviours: Caching, CacheInvalidation, Validation, Logging, ExceptionHandling
- [x] Auth command records (`LoginCommand`, `CreateAppUserCommand`, `RefreshTokenCommand`, etc.)
- [x] Customer CQRS: `CreateCustomerCommand`, `UpdateCustomerCommand`, `DeleteCustomerCommand`, get/list/search queries
- [x] Employee CQRS: `CreateEmployeeCommand`, `GetEmployeesQuery`
- [x] Cache key helpers (`CacheKeys` static class)
- [x] `ICachableRequest` / `ICacheInvalidatorRequest` interfaces
- [x] `IIntegrationEventPublisher`, `IEncryptionService`, `IBackgroundJobService` contracts
- [x] LoggerMessage EventId range 2000–2323

### ✅ Phase 4 — Infrastructure Layer
- [x] IAM module: Identity command handlers (Login, CreateAppUser, RefreshToken, Logout, TOTP, OTP flows, ResetPassword, etc.)
- [x] IAM module DI (`IamModuleDI.cs`)
- [x] Banking module, HR module extensions
- [x] MassTransit consumers: `CustomerCreatedEventConsumer`, `SendWelcomeEmailConsumer`
- [x] Hangfire background job: `MediatorSerializedJob`
- [x] `IServiceManager` with `ICacheService`, `ISessionService`
- [x] LoggerMessage EventId range 3000–3466

### ✅ Phase 5 — Persistence Layer
- [x] EF Core DbContexts (Banking, HR, IAM, Shared)
- [x] All repository implementations
- [x] UoW implementations with domain-event dispatch
- [x] Entity configurations (Fluent API)
- [x] Seed data
- [x] `ISharedUnitOfWork` / `IPersistenceSharedExtensions`
- [x] LoggerMessage EventId range 4000–4204

### ✅ Phase 6 — API Layer
- [x] `Program.cs`: Azure Key Vault, Serilog, CORS, OpenAPI/Scalar, health checks
- [x] `BaseController` with `HandleResponse<T>` and Problem Details error mapping
- [x] `CustomerController` (Create)
- [x] `EmployeeController` (Create)
- [x] `TotpController` (scaffolded — empty)
- [x] Middleware wiring (pre-auth, post-auth, security headers, rate limiter)
- [x] LoggerMessage EventId range 1000–1002

### 🔧 Phase 7 — Complete API Endpoints _(in progress)_
- [ ] Add `AuthController`: Login, Logout, RefreshToken, CreateAppUser, ResetPassword, SendEmailOtp, VerifyEmailOtp, VerifyOtp, VerifyPassword, GetCurrentUser, GetOtpStatus, InitiateTotpSetup, GrantEmployeeSystemAccess, LinkCustomerToUser, LinkEmployeeToUser
- [ ] Complete `TotpController`: InitiateTotpSetup, VerifyTotpCode endpoints
- [ ] Migrate `CustomerController` and `EmployeeController` to use `HandleResponse<T>` instead of raw `Ok`/`BadRequest`
- [ ] Resolve `CreateCustomer.cs` TODO: inject `IUserContextService` for `currentuser`

### 📋 Phase 8 — Blazor Web App
- [ ] Service registration and HttpClient setup
- [ ] Authentication state provider (JWT)
- [ ] Auth pages: Login, 2FA, Reset Password
- [ ] Customer pages: List, Detail, Create/Edit
- [ ] Employee pages: List, Detail
- [ ] Dashboard

### 📋 Phase 9 — MAUI Mobile App
- [ ] Navigation shell (AppShell)
- [ ] Auth flow: Login → 2FA (TOTP/Email OTP) → token persistence
- [ ] API client (`IApiClient` / Refit)
- [ ] Customer pages
- [ ] Employee pages
- [ ] Offline support / resilience

### 📋 Phase 10 — Tests & CI/CD
- [ ] Unit tests for Application command handlers (mock UoW)
- [ ] Unit tests for domain entity invariants
- [ ] Integration tests (TestContainers / in-memory EF)
- [ ] GitHub Actions workflow: build → architecture tests → unit tests → integration tests
- [ ] Docker / deployment configuration
- [ ] README documentation

---

## Known Issues / TODOs

| File | Issue | Priority |
|------|-------|----------|
| `CreateCustomer.cs` | TODO: "Should get currentuser" when building Customer aggregate | Medium |
| `CustomerController.cs` | Uses `BadRequest`/`Ok` directly instead of `HandleResponse<T>` | Low |
| `EmployeeController.cs` | Uses `BadRequest`/`Ok` directly instead of `HandleResponse<T>` | Low |
| `TotpController.cs` | Empty — TOTP endpoints not yet implemented | High |
| No `AuthController` | All auth commands have handlers but no HTTP surface | High |

---

## Key Decisions Log

| Date | Decision | Rationale |
|------|----------|-----------|
| Project start | Auth handlers live in Infrastructure (not Application) | They require Identity services (`UserManager`, `SignInManager`) which are infrastructure concerns |
| Project start | Web UI uses `ISender` directly | Eliminates redundant façade layer, improves testability |
| Project start | Mobile uses API endpoints only | Enforces proper separation; mobile should not bundle Application layer |
| Project start | `ExecuteInTransactionWithRetryAsync` over manual transactions | Built-in retry with exponential back-off; reduces boilerplate |
| Project start | EventId range governance by layer | Enables precise log filtering and monitoring alerts per layer |
| 2026-04 | MAUI scaffolded as part of base template | Mobile app is a first-class consumer, not an afterthought |

---

## How to Onboard a New Session

Every Copilot session auto-loads `.github/copilot-instructions.md` which mirrors the key rules from this document. For additional context:

1. Read this `PLAN.md` to understand current phase status.
2. Check the latest merged PRs for detailed change history.
3. Run `git log --oneline -20` to see recent commits.
4. Architecture tests in `BT.Tests.Architecture` act as living documentation of dependency rules.
