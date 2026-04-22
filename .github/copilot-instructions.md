# Copilot Instructions

---

## 1. Project Identity

| Item | Value |
|------|-------|
| Repo | `odhiamboally/base-template` |
| Root namespace prefix | `BT` (e.g. `BT.Api`, `BT.Application`) |
| Service/display name | `LlanCore.BaseTemplate.API` |
| Solution file | `BaseTemplate.slnx` |
| Target framework | Latest .NET LTS (currently .NET 9) |
| C# version | Latest (`<LangVersion>latest</LangVersion>`) |
| Secret config convention | Azure Key Vault double-hyphen: `Section--Setting` (maps to `Section:Setting`) |

---

## 2. Solution Layout

```
src/
  Backend/
    Api/         BT.Api            ← HTTP entry point
    Application/ BT.Application    ← CQRS handlers, behaviours, contracts
    Domain/      BT.Domain         ← Entities, value objects, domain events, repository interfaces
    Infrastructure/ BT.Infrastructure ← IAM/Identity, Banking, HR, Messaging, Jobs
    Persistence/ BT.Persistence    ← EF Core, UoW implementations, seeds
  Frontend/
    Mobile/      BT.UI.Maui        ← .NET MAUI app (mobile consumers go via API endpoints)
    Web/         BT.UI.Blazor      ← Blazor web app (web consumers use ISender directly)
  Shared/
    BT.SharedKernel               ← DTOs, enums, common configurations
    BT.SharedKernel.Validation    ← FluentValidation rule sets
tests/
  BT.Tests.Architecture           ← NetArchTest dependency/naming enforcements
  BT.Tests.Unit
  BT.Tests.Integration
```

---

## 3. Clean Architecture Rules (ENFORCED by BT.Tests.Architecture)

Dependency direction: **Domain ← Application ← Infrastructure/Persistence ← Api**

- Domain must NOT reference Application, Infrastructure, or Persistence.
- Application must NOT reference Infrastructure or Persistence — communicate only through `IUnitOfWork` / `IRepository` abstractions in Domain.
- SharedKernel must NOT reference Domain, Infrastructure, or Persistence.
- Infrastructure and Persistence MAY reference Domain and Application.
- Any violation of these rules will fail the architecture tests in CI.

---

## 4. CQRS Conventions

### General Rule
Command/query record **and** its handler class live in the **same `.cs` file**.

### Application layer (non-Identity features)
Folder: `BT.Application/Features/{Domain}/{CommandHandlers|QueryHandlers}/`
- File name matches the operation: `CreateCustomer.cs`, `GetCustomerById.cs`
- Both `CreateCustomerCommand` (record) and `CreateCustomerCommandHandler` (class) in the same file.

### Infrastructure layer (Identity / ASP.NET Core Identity handlers)
Auth command records live in: `BT.Application/Features/Auth/Commands/`
Auth handlers live in: `BT.Infrastructure/Features/Auth/AspNetCoreIdentity/CommandHandlers/`
- This is intentional — Identity services (`UserManager`, `SignInManager`) belong in Infrastructure.
- The command record (contract) stays in Application; the handler (implementation) is in Infrastructure.

### Naming
- Commands: `{Verb}{Entity}Command` — e.g., `CreateCustomerCommand`
- Queries: `Get{Entity}Query` / `Search{Entity}Query`
- Handlers: `{Command/Query}Handler` suffix (sealed internal class)
- Do NOT use `CommandHandler` / `QueryHandler` as folder names when the convention is already `CommandHandlers` / `QueryHandlers`.

---

## 5. LoggerMessage EventId Ranges (STRICT governance)

| Layer | Project | Range |
|-------|---------|-------|
| **API** | `BT.Api` | 1000–1999 |
| **Application** | `BT.Application` | 2000–2999 |
| **Infrastructure** | `BT.Infrastructure` | 3000–3999 |
| **Persistence** | `BT.Persistence` | 4000–4999 |
| **Domain** | `BT.Domain` | 5000–5999 _(reserved)_ |

Rules:
- Always use `[LoggerMessage(EventId = N, ...)]` source-generated methods — never `logger.Log(...)` free-form calls in hot paths.
- Use `internal static partial class` in the correct layer's `Logging/` folder.
- Never reuse an EventId within the same range.
- Current highest used per layer: API ~1002, Application ~2323, Infrastructure ~3466, Persistence ~4204.

---

## 6. UnitOfWork / Transaction Pattern

**Always prefer** `ExecuteInTransactionWithRetryAsync` over manual `BeginTransactionAsync`/`RollbackTransactionAsync`.

```csharp
// ✅ Preferred
await _unitOfWork.ExecuteInTransactionWithRetryAsync(async () =>
{
    await _unitOfWork.SomeRepository.CreateAsync(entity, ct);
    return true;
});

// ❌ Avoid
var tx = await _unitOfWork.BeginTransactionAsync();
try { ... await tx.CommitAsync(); }
catch { await tx.RollbackTransactionAsync(); }
```

UoW interfaces (all extend `ITransactionalUnitOfWork`):
- `IBankingUnitOfWork` — customer/banking domain
- `IHrUnitOfWork` — employee/HR domain
- `IIamUnitOfWork` — identity/auth domain
- `ISharedUnitOfWork` — cross-cutting / lookup data

---

## 7. Auth / ISender Architecture

- **Web consumers** (BT.UI.Blazor): call Application handlers directly via `ISender` — NO façade `AuthService`.
- **Mobile consumers** (BT.UI.Maui): MUST go through API endpoints, never call handlers directly.
- Façade services (e.g., `AuthService`) are being phased out. New features must use `ISender`.

---

## 8. API Controller Conventions

- All controllers inherit `BaseController` and are `internal sealed class`.
- Use primary constructor injection with `ISender sender`.
- Use `HandleResponse<T>(AppResponse<T> response)` from `BaseController` for all responses (do NOT return raw `BadRequest`/`Ok` directly).
- Controllers are versioned: `[ApiVersion("1.0")]`.
- Controllers are in `BT.Api/Controllers/` — never in subdirectories.

**Known gap (to be fixed):** `CustomerController` and `EmployeeController` currently return raw `BadRequest`/`Ok` instead of `HandleResponse` — migrate these when touching those files.

---

## 9. Response / Error Pattern

All handler return types: `AppResponse<T>` (from `BT.SharedKernel.Dtos.Common`).
- `AppResponse.Success(message, data)` — successful result.
- `AppResponse.Failure<T>(message)` — failed result (non-exceptional).
- Throw exceptions for truly unexpected/unrecoverable errors (let the pipeline behaviour handle them).
- `HandleResponse<T>` in `BaseController` maps to HTTP Problem Details for failures.

---

## 10. Caching

- `ICachableRequest` — for cacheable queries (implement `CacheKey`, `Expiry`, `BypassCache`).
- `ICacheInvalidatorRequest` — for commands that invalidate cache (implement `DirectInvalidationKeys`, `GroupVersionKeysToInvalidate`).
- Cache keys generated via `CacheKeys` static helper in `BT.Application.Utilities`.
- Pipeline behaviours `CachingBehavior` and `CacheInvalidationBehavior` handle automatically via MediatR pipeline.

---

## 11. Messaging / Background Jobs

- **MassTransit** for integration events / message bus.
- Consumers in: `BT.Infrastructure/Messaging/Consumers/`.
- Integration event contracts in: `BT.Application/IntegrationEvents/`.
- **Hangfire** for background/scheduled jobs. Serialized MediatR commands dispatched via `MediatorSerializedJob`.

---

## 12. Frontend (MAUI)

- MAUI project: `BT.UI.Maui` in `src/Frontend/Mobile/`.
- Communicates with backend exclusively via API endpoints — no direct DI of Application or Domain.
- Auth flows: login → 2FA (TOTP/Email OTP) → token storage.

---

## 13. Current Project State (as of 2026-04-22)

### ✅ Completed
- Domain layer: entities, value objects, domain events, repository and UoW interfaces
- Application layer: CQRS structure, pipeline behaviours (caching, validation, logging, exception handling), auth commands
- Infrastructure: IAM/Identity handlers (Login, CreateAppUser, RefreshToken, TOTP, OTP, etc.), Banking module, HR module, Messaging consumers, Hangfire job
- Persistence: EF Core DbContexts, all repository implementations, UoW implementations, entity configurations, seed data
- API: Program.cs (Azure Key Vault, Serilog, CORS, OpenAPI/Scalar, health checks), BaseController, CustomerController, EmployeeController
- Architecture tests: layer dependency, naming conventions, persistence layer rules
- LoggerMessage governance: EventId ranges established and populated

### 🔧 In Progress / Known Gaps
- `TotpController` — created but empty; TOTP endpoints (initiate setup, verify code) need wiring
- `CustomerController` / `EmployeeController` — use raw `Ok`/`BadRequest` instead of `HandleResponse<T>`
- Auth commands (Login, RefreshToken, etc.) — no corresponding API controller exists yet; these need an `AuthController`
- `CreateCustomer.cs` — TODO comment: "Should get currentuser" when building the Customer aggregate
- `BT.UI.Blazor` — scaffolding in place; service layer and component build-out pending
- `BT.UI.Maui` — scaffolding in place; navigation, auth flow, and API client wiring pending

### 📋 Upcoming Work
1. Add `AuthController` to wire all auth commands (Login, Logout, RefreshToken, CreateAppUser, ResetPassword, etc.)
2. Complete `TotpController` endpoints
3. Migrate `CustomerController` and `EmployeeController` to use `HandleResponse<T>`
4. Resolve `currentuser` TODO in `CreateCustomer.cs` (inject `IUserContextService` or read from command)
5. Build out Blazor web app service and component layer
6. Build out MAUI mobile app navigation, auth, and API client
7. Add unit and integration tests for handlers
8. CI/CD pipeline (GitHub Actions) setup

---

## 14. Standing Rules for All Sessions

- Service naming: always `LlanCore.BaseTemplate.*` — never `Onboarding.*`.
- Secret config keys: double-hyphen convention (e.g., `Jwt--Key`, `ConnectionStrings--DefaultConnection`).
- New CQRS files: command + handler in the same file; folder per domain in `Features/`.
- LoggerMessage: always use source-generated `[LoggerMessage]`; respect EventId ranges; no free-form log calls in new code.
- Transactions: always use `ExecuteInTransactionWithRetryAsync`.
- Web consumers: always use `ISender`; no façade services.
- Explain tool/command intent before running commands.
- Do not modify unrelated code; keep changes surgical.

---

## Command Usage
- Explain command intentions before running commands/tools.