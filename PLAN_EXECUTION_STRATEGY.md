# Base Template - Execution Strategy

> Last updated: 2026-06-03
>
> This file explains how to execute `PLAN.md` without jumping between unrelated work.
> `PLAN.md` defines what the template should become. This file defines sequencing, dependencies, readiness gates, and safe parallel work.

---

## 1. Purpose

The BaseTemplate roadmap is intentionally broad. Many items depend on other items being complete enough before they can integrate well.

This strategy prevents three common problems:

- Starting advanced features before the foundation they depend on exists.
- Reworking earlier code because dependent phases were implemented in the wrong order.
- Spending time on maturity polish before core production flows are working.

The goal is steady progress through working vertical slices.

---

## 2. Execution Principles

- Prefer completing one working vertical slice over touching many incomplete areas.
- Do not start a dependent feature until its prerequisite is implemented to the readiness gate defined here.
- Core correctness, security, and data integrity are not maturity tasks.
- Maturity tasks are allowed only when the current implementation is production-safe but not maximally configurable.
- Every meaningful implementation should update `PLAN.md` or this file when the roadmap or dependency understanding changes.
- Every meaningful implementation should end with a concise commit message.
- Local build/test/architecture checks should run whenever practical before calling work complete.
- Persistence work should follow `docs/architecture/persistence-standards.md`.

---

## 3. Critical Path

This is the default execution path unless a blocker forces a dependency detour.

1. Local running solution
2. IAM/authentication completeness
3. Customer CRUD reference flow
4. Exception handling and validation consistency
5. Pipeline behaviours and transaction conventions
6. RBAC/ABAC/permission-based authorization
7. Feature flags
8. Dynamic menu/module loading
9. Health checks, audit, observability, and security hardening
10. Docker/local developer platform
11. Azure deployment, Key Vault, monitoring, and APIM
12. Entra ID SSO
13. Rich reusable template capabilities: SignalR, reporting, payment abstractions, mobile

Current near-term focus:

- Keep the local API and Blazor UI runnable.
- Finish the Customer reference flow as the pattern for future features.
- Complete IAM and authorization enough that menu visibility and admin features can be secured properly.

---

## 4. Dependency Map

| Work Item | Depends On | Why |
| --- | --- | --- |
| Blazor authenticated shell | IAM login, token/session handling | The shell needs reliable current-user state. |
| Customer CRUD reference flow | API auth, customer endpoints, DTO contracts | It proves UI to API to Application to Persistence and back. |
| Admin users/roles/permissions UI | IAM API completeness, authorization model | Admin screens must manage real identity data, not placeholders. |
| RBAC/ABAC/permission model | IAM users, roles, claims, policies | Permissions need stable user/role/claim primitives. |
| Direct user permission grants | Permission model | Exceptions beyond role membership need a permission catalog first. |
| Dynamic menus from DB | RBAC/permission model, feature flags | Menus must be filtered by real authorization and feature availability. |
| Feature-gated UI | Feature flag abstraction | UI visibility must use the same feature gate model as API/Application. |
| Endpoint authorization policies | IAM claims and permission model | Policies need known claims/permissions to evaluate. |
| Audit trail | Current user/context resolution | Audit records must know who performed an action. |
| Soft delete consistency | EF configurations, repository/UoW conventions | Query filters and delete behavior must be enforced centrally. |
| Health checks | Dependency configuration | Checks need known SQL, Redis, messaging, Seq, Key Vault, and provider settings. |
| Observability | Logging conventions, health/dependency map | Traces/metrics/logs need stable operation boundaries. |
| Docker Compose | Local dependency settings | Compose should match actual appsettings and ports. |
| Azure deployment | Local run, CI, config/secrets, health checks | Deployment should not be automated before the app can prove readiness. |
| Key Vault | Configuration model | Secrets need established settings POCOs and section names. |
| Entra ID SSO | IAM AppUser linking model | External identities must map into local users/employees safely. |
| Profile media | Storage abstraction, IAM/HR/Banking ownership rules | Account avatars and business-domain photos/documents must not become one mixed upload bucket. |
| Kenya verification providers | Customer/Employee identity model, resilience, audit | Verification APIs need stable identity fields, provider adapters, audit, and secure PII handling. |
| SignalR | Auth and user/group strategy | Hubs need secure user, role, tenant, and context grouping. |
| QuestPDF reporting | Feature data and storage/download strategy | Reports should be generated from real features and delivered consistently. |
| Payments | Domain feature needing payments, provider abstraction | Provider integrations need idempotency, audit, callbacks, and reconciliation. |
| Product modules such as loans | BaseTemplate completed and cloned downstream | SACCO/product-specific behavior belongs in the downstream solution, not the reusable template core. |

---

## 5. Readiness Gates

### Gate 1 - Local Running Solution

Required before heavy feature expansion:

- API runs locally from Visual Studio.
- Blazor UI runs locally.
- SQL Server connection is configured.
- Development seed/admin login works.
- Scalar/OpenAPI is reachable.
- Relevant local build commands pass.

### Gate 2 - IAM/Auth Ready

Required before protected admin modules, dynamic menus, and authorization-heavy UI:

- Login works through API.
- Refresh token flow works.
- Logout clears local session.
- Current-user endpoint returns stable user identity, roles, claims, and context links.
- TOTP setup and verification work.
- Employee and Customer AppUser links are demonstrable.
- Critical auth paths have tests or documented manual verification.

### Gate 3 - Customer Reference Flow Ready

Required before cloning patterns into Employees, Users, Roles, Permissions, and future business modules:

- Customer list loads real DB records.
- Search, filters, pagination, view, create, edit, and delete work.
- Delete uses confirmation.
- User-facing errors are meaningful.
- API failures are logged through source-generated LoggerMessage.
- Shared API client and endpoint configuration are used.
- UI table/action/dialog conventions are reusable.

### Gate 4 - Authorization Ready

Required before dynamic menus and permission-managed admin surfaces:

- Permission catalog exists.
- Roles can be assigned permissions.
- Users can receive direct permission grants where needed.
- Policies or permission attributes protect APIs.
- Application-level checks exist where endpoint checks are not enough.
- UI can query allowed capabilities for the signed-in user.
- Tests cover role permission, direct grant, forbidden, and unauthorized cases.

### Gate 5 - Feature Flags Ready

Required before feature-gated modules and rollout flows:

- Application-owned feature gate abstraction exists.
- Local configuration supports flags.
- Azure App Configuration compatibility is preserved.
- API/Application/UI can evaluate flags consistently.
- Disabled features fail closed where appropriate.

### Gate 6 - Dynamic Menus Ready

Required before removing static admin/menu definitions:

- Menu/module catalog is modeled and persisted.
- Nested menu items are supported.
- Menus are filtered by permission, role, feature flag, tenant/context where applicable.
- Direct URL access remains protected by server-side policies.
- Menu cache invalidation is defined.

### Gate 7 - Deployment Ready

Required before Azure deployment automation is considered complete:

- CI builds and tests relevant projects.
- Health checks include real dependencies.
- Secrets are externalized.
- Release workflow builds deployable artifacts.
- Deployment has smoke tests and rollback guidance.
- Logs/traces/metrics are visible in the selected monitoring stack.

---

## 6. Safe Parallel Work

These can be worked on in parallel when they do not block or destabilize the critical path:

- Documentation updates.
- CI workflow naming and small workflow improvements.
- UI theme polish that does not rewrite routing/auth/data flow.
- Architecture tests for conventions already agreed.
- Local run documentation.
- Docker Compose after local dependency settings are stable.
- OpenAPI/Scalar improvements.
- Reporting abstractions before full business reports.
- Background job examples after configuration is stable.

Parallel work must still integrate through normal build/test checks.

---

## 7. Work To Avoid Too Early

Avoid these until their gates are ready:

- DB-backed dynamic menus before permissions and feature flags exist.
- Admin permission UI before permission model behavior is clear.
- External verification provider integrations before Customer/Employee identity evidence and audit models are stable.
- Azure deployment before local run, health checks, and secret strategy are stable.
- Entra ID SSO before AppUser linking and claims model are stable.
- SACCO/product modules before BaseTemplate platform foundations are reliable and cloned downstream.
- Plugin-driven modules before static/dynamic module catalog is proven.
- Heavy maturity polish before a working local product demo exists.

---

## 8. Session Workflow

At the start of a session:

1. Read `PLAN.md`.
2. Read this file.
3. Check `git status`.
4. Identify the current critical-path gate.
5. Choose the next task that satisfies or unblocks that gate.

During a session:

1. Prefer one bounded task.
2. Avoid changing unrelated layers unless the dependency requires it.
3. Update the user when a dependency or sequencing assumption changes.
4. Keep convention changes documented.

At the end of a session:

1. Run relevant checks where practical.
2. Update `PLAN.md` or this file if scope changed.
3. Generate a concise commit message.
4. Note any remaining blocker clearly.

---

## 9. Current Next Moves

Recommended immediate order:

1. Finish Customer CRUD reference flow completely.
2. Apply the proven UI/API/service conventions to Employees, Users, Roles, and Permissions.
3. Complete IAM authorization model: roles, permissions, policies, direct user grants.
4. Add feature flag abstraction.
5. Replace static admin/menu definitions with backend-provided permission-aware menu data.
6. Complete remaining reusable platform services: profile media storage abstraction, health/security/observability, feature flags, background jobs, and deployment hardening.
7. Move product-specific work such as SACCO loans, KYC/CRB, AML, payroll, or full HR into downstream solutions cloned from BaseTemplate.
