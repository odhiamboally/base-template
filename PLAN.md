# Base Template - Living Architecture And Delivery Plan

> Last updated: 2026-06-06
>
> This file is the canonical roadmap for the BaseTemplate solution.
> Update it at the end of every meaningful architecture, feature, CI/CD, or deployment work session.

---

## 1. Purpose

BaseTemplate is a reusable enterprise .NET solution template for building production-grade systems quickly without starting from a weak starting point.

The solution is intentionally designed as a Modular Monolith first:

- One deployable unit while the domain is still evolving.
- Multiple bounded contexts with strict ownership.
- Clean Architecture dependency direction.
- Vertical feature folders across layers for navigability.
- DDD-friendly domain model with explicit EF Core configuration.
- Reliable integration events through MassTransit EF outbox.
- CI/CD guardrails that prevent architectural drift.
- Azure-ready deployment, monitoring, security, and operations path.

This template should be strong enough to clone for future projects and disciplined enough that future contributors know where every concern belongs.

---

## 2. Architectural Direction

### 2.1 Core Architecture

The chosen architecture is:

- Modular Monolith
- Clean Architecture
- Bounded Contexts
- Vertical Slice / Feature folders
- CQRS with MediatR
- DDD-oriented aggregates and value objects
- EF Core with repository and Unit of Work abstractions
- Domain events for in-process behavior
- Integration events plus MassTransit EF outbox for cross-context behavior

MediatR remains the mediator package for now. A future move to an MIT-licensed mediator can be planned separately if licensing or package policy requires it.

### 2.2 Bounded Contexts

Current platform/reference bounded contexts:

- IAM: identity, authentication, authorization, AppUser lifecycle, tokens, sessions, MFA/TOTP, claims.
- Workforce: minimal staff/employee identity records needed for IAM, ownership, approvals, and organizational access rules. This is not a full HR/payroll module.
- Customer Management: generic customer/person or organization records used as a reference business flow and IAM linkage example. Product-specific onboarding, KYC, loans, and servicing belong in downstream applications.
- Shared: lookups, email templates, failed messages, shared infrastructure records, MassTransit outbox ownership.

Rule: cross-context references should be by ID or integration event. Avoid direct domain navigation across bounded contexts.

### 2.3 Feature Folder Convention

Feature-owned code should live under:

```text
Features/{BoundedContext}/{Feature}
```

Use this convention across layers where feature ownership improves navigability:

- Domain
- Application
- Infrastructure
- Persistence
- API
- Shared DTO/validation projects
- Frontend projects when feature-specific

Keep cross-cutting code outside `Features`, for example:

- logging
- middleware
- security headers
- generic caching
- generic repository base types
- common abstractions
- base entities
- JSON utilities
- OpenTelemetry setup
- CI/CD scripts

---

## 3. Current State

### 3.1 Architecture Work Completed

The following architecture restructuring work is complete:

- Backend solution split into Domain, Application, Infrastructure, Persistence, and API layers.
- Shared kernel and validation projects exist.
- Blazor Web, RCL, and MAUI projects exist.
- Bounded-context folder structure exists across the major backend layers.
- Customer naming is standardized over Client/Member naming.
- IAM, HR, Banking, and Shared DbContexts exist.
- Bounded-context Unit of Work interfaces and implementations exist.
- Bounded-context repositories are feature-owned.
- EF entities with DbSet declarations are required to have explicit configurations.
- MassTransit EF outbox uses the Shared DbContext.
- Custom domain `OutboxMessage` was removed in favor of MassTransit outbox.
- Domain events and integration events are context-bounded.
- Customer and Employee are aligned to AppUser through IAM-owned access fields.
- AppUser supports EmployeeId and CustomerId links.
- Cacheable queries and cache-invalidating commands are guarded by architecture tests.
- Application query cache keys have been aligned for customer, employee, lookup, and dashboard flows.
- LoggerMessage usage is established as the logging convention.
- Local pre-push hooks and GitHub Actions architecture checks are wired.
- Branch protection is configured on `main`.

### 3.2 Areas That Must Still Be Completed

The goal is not to leave these as half-finished building blocks. Each item below must reach a working, documented, tested state.

- Full IAM API surface, including auth endpoints and TOTP endpoints.
- Complete RBAC/ABAC/permission-based authorization model.
- Dynamic navigation/menu composition based on user permissions and feature availability.
- Complete frontend auth/session flow.
- Complete MAUI API-client/mobile auth flow.
- Full exception hierarchy and RFC 7807 Problem Details mapping.
- Health checks for actual dependencies, not only basic API liveness.
- Audit trail beyond simple audit columns.
- Feature flags.
- Entra ID / Azure AD OIDC SSO.
- OWASP Top 10 security checklist and automated guardrails.
- SignalR.
- Reporting with QuestPDF.
- Payment gateway abstraction, with concrete providers added only if they remain reusable across downstream applications.
- Docker and Docker Compose for local dependencies.
- Azure deployment workflow.
- Azure monitoring and operational runbooks.
- API Management integration path.

### 3.3 Document Review Findings To Carry Forward

The legacy architecture/refactoring documents in `D:\Downloads\Documents\Base Template` remain useful, but some product-specific or older assumptions have been superseded by the BaseTemplate-as-platform decision.

Carry forward into BaseTemplate:

- Keep the Modular Monolith, Clean Architecture, bounded-context DbContexts, BC-scoped UnitOfWork, feature folders, and MassTransit outbox direction.
- Remove password handling from Employee/Workforce creation. System access must be granted through IAM activation/provisioning flows.
- Replace placeholder audit stamping such as `"System"` with a real current-user/system-user context abstraction.
- Add `BaseAggregateRoot` or equivalent distinction between aggregate roots and simple entities.
- Complete exception hierarchy and RFC 7807 ProblemDetails mapping.
- Complete security, health, observability, feature flags, storage, background jobs, Docker/local platform, Azure deployment, and CI/CD hardening.
- Keep package/version governance, architecture tests, analyzers, and code-style rules as first-class template guardrails.

Do not carry into BaseTemplate core:

- Full HR/payroll assumptions.
- SACCO/product modules such as loans, KYC/CRB workflows, AML screening, guarantors, servicing, or core banking.
- Country-specific verification providers as built-in implementations. Keep only the extension pattern and abstractions.

Needs decision:

- Whether lightweight Customer should remain a generic reference flow or be renamed to a more neutral sample/domain seed in the future.
- Whether `HireDate` and `PositionId` belong in the lightweight Workforce Identity model or should be removed until a downstream HR module owns them.
- Whether to introduce Aspire in addition to Docker Compose, or postpone Aspire until local Docker/deployment foundations are stable.

---

## 4. Engineering Conventions

### 4.1 Naming

- Avoid redundant contextual property names inside contextual types.
- Prefer `Customer.Number` over `Customer.CustomerNumber`.
- Prefer `Employee.Number` over `Employee.EmployeeNumber`.
- Use `Customer`, not `Client` or `Member`.
- Preserve clear names when removing context would reduce meaning.

### 4.2 Query Performance

- Prefer `Any()` over `Count() > 0` for existence checks.
- Keep analyzer guidance in `Directory.Build.props`.
- If an analyzer incorrectly complains about `Any()`, document the exception and fix the analyzer configuration rather than changing good code to worse code.

### 4.3 EF Core

- Every persisted DbSet entity must have an explicit `IEntityTypeConfiguration<T>`.
- Use bounded-context DbContexts.
- Keep repository and Unit of Work abstractions.
- Use Unit of Work transaction helpers rather than manual transaction boilerplate.
- Soft-delete behavior must be consistent across contexts.
- Audit stamping must use the current user context, not placeholders like `System`, except for true system operations.

### 4.4 Logging

- Use source-generated `LoggerMessage`.
- Every catch block must log or intentionally rethrow without swallowing.
- Keep EventId ranges by layer.

EventId ranges:

| Layer | Range |
| --- | --- |
| API | 1000-1999 |
| Application | 2000-2999 |
| Infrastructure | 3000-3999 |
| Persistence | 4000-4999 |
| Domain | 5000-5999 |

### 4.5 Configuration

- Every settings POCO must have a corresponding JSON configuration section.
- Every JSON configuration section should bind to a known POCO or be intentionally documented as framework-owned.
- Secrets must not live in production appsettings.
- Azure Key Vault uses the double-hyphen convention: `Section--Setting`.

### 4.6 Commit Messages

At the end of every meaningful work session, generate a concise commit message that captures the completed change.

Commit messages should be:

- short but descriptive
- focused on user-visible or architecture-visible outcome
- not a raw file list

Example:

```text
Align persistence modules with bounded-context feature structure
```

---

## 5. CI/CD And Guardrails

### 5.1 Current CI/CD State

The repository has intent-specific GitHub Actions workflows:

- `backend-architecture.yml`
- `backend-ci.yml`
- `frontend-ci.yml`
- `mobile-ci.yml`
- `full-solution-ci.yml`
- `release.yml`

Local guardrails:

- `scripts/checks.ps1`
- `.githooks/pre-push`

Branch protection:

- direct pushes to `main` are blocked
- PR checks must pass
- conversations must be resolved

### 5.2 GitHub Actions Extension Rule

New workflows should be named by intent, not by vague technology labels.

Good workflow names:

- `backend-ci.yml`
- `frontend-ci.yml`
- `mobile-ci.yml`
- `backend-architecture.yml`
- `security-scan.yml`
- `docker-publish.yml`
- `azure-appservice-deploy.yml`
- `database-migrations.yml`

Each workflow should answer:

- What does this protect or deliver?
- When should it run?
- Is it required for PR merge?
- Does it build, test, scan, package, deploy, or notify?
- Is it reusable via `workflow_call`?
- Does it use Debug for local speed or Release for deployable confidence?

### 5.3 Target CI/CD Flow

Pull request flow:

1. Developer works on a feature branch.
2. Local pre-push guardrails run.
3. GitHub PR checks run.
4. Architecture checks verify conventions.
5. Backend/frontend/mobile checks run according to changed paths.
6. Security and dependency checks run.
7. Review is completed.
8. PR merges to `main`.

Main branch flow:

1. Full solution checks run.
2. Release packaging runs.
3. Docker image or deployable artifact is produced.
4. Azure deployment workflow deploys to the selected environment.
5. Smoke tests and health checks run.
6. Observability dashboards and alerts confirm the deployment is healthy.

### 5.4 Required Future Workflows

- Security scan workflow.
- Dependency vulnerability workflow.
- Docker build/publish workflow.
- Azure App Service deployment workflow.
- Database migration workflow or documented manual migration process.
- Release tagging workflow.

---

## 6. Capability Roadmap

### Phase A - Finish API And IAM Completeness

Goal: IAM should be fully usable, not only structurally present.

Status:

- Automated implementation and guardrails are complete; the phase is ready for local browser smoke testing from Visual Studio.
- The auth/token/session spine is hardened.
- Auth and TOTP API endpoints have been exposed for the existing IAM command handlers.
- Login and TOTP completion now create a server-side session, place the session id in JWT/current-user responses, persist refresh tokens, and return the session id to the Blazor client.
- Blazor stores the session id with the access/refresh tokens and sends it as `X-Session-Id` on authenticated API calls.
- Refresh-token rotation validates the active session, persists the replacement token, marks the old token used, and uses configured refresh-token expiry.
- Logout revokes the active session and active refresh tokens server-side.
- Dynamic permission policies and `[RequirePermission]` are wired for IAM/admin, customer, employee, department, lookup, and menu endpoints.
- TOTP status/setup endpoints have been tightened so status is authenticated and admin setup requires user-management permission.
- MFA enrollment policy is configuration-backed under `SecuritySettings:Mfa`; required users are flagged in auth/current-user responses and blocked from normal protected APIs until enrollment is completed.
- Blazor session state exposes role/permission helpers, redirects MFA-required users to security setup, and hides admin action buttons when the user lacks matching permissions.
- Employee Grant System Access creates or reactivates the linked AppUser, assigns roles, enforces password change, and sends an activation email through the configured email service.
- Employee Revoke System Access is exposed as a first-class admin action and deactivates the linked AppUser while terminating active sessions and refresh tokens.
- Permission authorization and MFA-enrollment middleware are covered by unit tests; API authorization guardrails are covered by architecture tests.
- Local SQL Server has the latest IAM migration applied for reference-catalog soft-delete alignment.
- Remaining manual check is a browser smoke test from Visual Studio: sign in, confirm MFA-required redirect/security page, open Admin Center, verify grant/revoke access, and confirm permitted actions render correctly.

Tasks:

- Add `AuthController`.
- Complete `TotpController`.
- Expose login, logout, refresh token, create app user, reset password, email OTP, TOTP setup, TOTP verify, current user, OTP status, password verification, employee access grant, customer/user linking, employee/user linking.
- Confirm JWT claims include EmployeeId, CustomerId, roles, and active context where applicable.
- Confirm Customer and Employee access flows work end-to-end.
- Add cookie auth plan for Blazor Server sessions if the web app should use cookie-based auth instead of only JWT storage.
- Add endpoint-level authorization policies.
- Build the authorization model for roles, permissions, claims, policies, and contextual access.
- Support direct user permission grants for exception scenarios where a user needs access beyond their role.
- Define the convention for attributes and policies, for example `[Authorize]`, named policies, and permission-specific attributes.
- Ensure permission checks can work at API endpoint, Application request, and UI visibility levels.
- Add tests for role-based, permission-based, and direct user-permission access scenarios.
- Add integration tests for critical auth flows.

Done means:

- A developer can run the API and complete local login/MFA/token flows.
- Employee and Customer AppUser access flows are demonstrable.
- TOTP is fully usable through API endpoints.
- Role, permission, policy, and direct user permission checks are demonstrable and enforced.
- Backend, frontend, unit, architecture, and integration checks are green for the completed IAM slice.

### Phase B - Complete Exception Handling And Validation

Goal: all failures return consistent RFC 7807 responses.

Tasks:

- Define exception hierarchy for domain, validation, not found, conflict, unauthorized, forbidden, unavailable, and external provider failures.
- Map exceptions through `IExceptionHandler`.
- Ensure FluentValidation is wired through the MediatR pipeline.
- Add tests for exception-to-ProblemDetails mapping.
- Ensure controllers do not duplicate error-shaping logic.

Done means:

- API error responses are predictable, documented, and test-covered.

### Phase C - Complete Pipeline Behaviours

Goal: command/query behavior is enforced consistently.

Current behaviours:

- Validation
- Logging
- Exception handling
- Caching
- Cache invalidation

Tasks:

- Add performance behavior for slow request logging.
- Add transaction behavior for commands where appropriate.
- Decide transaction marker convention, for example `ITransactionalRequest`.
- Keep cache marker architecture tests.
- Add architecture tests for transactional command conventions once adopted.

Done means:

- Handlers stay thin and do not repeatedly implement cross-cutting concerns.

### Phase D - Health, Audit, Observability, And Operations

Goal: the system can be operated with confidence.

Tasks:

- Expand health checks for SQL Server, Redis, RabbitMQ or Azure Service Bus, Key Vault, Seq or log sink, and external providers where applicable.
- Separate liveness, readiness, and dependency checks.
- Complete audit trail strategy.
- Decide whether audit trail is column-based only, table-based, event-based, or a hybrid.
- Ensure audit stamping uses current user context.
- Strengthen OpenTelemetry traces, metrics, and logs.
- Add Azure Monitor dashboards and alert plan.
- Document runbooks for failed deployment, failed message, unhealthy dependency, and rollback.

Done means:

- A production operator can tell whether the system is healthy and why it is unhealthy.

### Phase E - Security Completion

Goal: OWASP Top 10 and enterprise security are first-class.

Tasks:

- Add OWASP Top 10 checklist document.
- Add security headers verification.
- Add CORS production-origin enforcement.
- Add rate limiting policies by endpoint class.
- Add authorization policy review.
- Add RBAC/ABAC/permission guardrail review.
- Add input validation review.
- Add secret scanning guidance.
- Add dependency vulnerability scanning.
- Add secure logging review to avoid leaking secrets/tokens/PII.
- Add CSRF strategy for cookie-authenticated UI paths.
- Add account lockout, password policy, token expiry, refresh-token rotation, and session revocation checks.

Done means:

- Security expectations are documented, automated where practical, and reviewed in CI/CD.

### Phase F - Feature Flags

Goal: features can be enabled gradually without deployments.

Recommended approach:

- Use `Microsoft.FeatureManagement`.
- Keep Azure App Configuration compatibility.
- Wrap it behind an application-owned abstraction such as `IFeatureGate`.
- Define feature names centrally.
- Support endpoint, request, UI, tenant/user, percentage, time-window, and environment-based flags.

Possible conventions:

- `IFeatureGatedRequest` for MediatR requests.
- Endpoint filters or attributes for APIs.
- UI helper service for Blazor/RCL rendering.
- Feature flag settings section for local development.

Done means:

- A feature can be dark-launched, enabled per environment, enabled per user/tenant, and gradually rolled out.

### Phase G - Entra ID / Azure AD SSO

Goal: corporate users can authenticate via OIDC while the app keeps its IAM model.

Tasks:

- Add Entra ID settings.
- Add Microsoft Identity Web or equivalent OIDC integration.
- Wrap MSAL/OIDC details behind IAM abstractions.
- Map external identity to AppUser.
- Define provisioning/linking behavior for Employee users.
- Preserve local Identity + TOTP where required.
- Document local auth vs corporate SSO flow.

Done means:

- Corporate users can sign in through Entra ID and receive correct AppUser/Employee claims.

### Phase G.1 - Identity Profile Media And Verification

Goal: user, employee, and customer identity evidence is handled securely without mixing IAM account concerns with business-domain documentation.

Design direction:

- IAM owns the signed-in account profile avatar stored on `AppUser.ProfilePictureUrl`.
- HR owns employee business profile photos and employment documents when those become part of staff records.
- Banking owns customer KYC photos, identity documents, and onboarding evidence.
- Store files in Azure Blob Storage or an equivalent provider through an application-owned storage abstraction.
- Store only URLs, blob names, hashes, metadata, status, and audit fields in SQL Server.
- Keep containers private by default and expose images/documents through authorized API access, signed URLs, or a controlled CDN policy.

Tasks:

- Add storage settings and `IFileStorageService` abstraction.
- Add Azure Blob implementation and local-development storage option.
- Add IAM profile-picture upload, replace, remove, and display flow.
- Add future HR employee photo/document model.
- Add future Banking customer KYC document model.
- Add file validation for content type, size, extension, malware-scanning hook, and audit trail.
- Add cleanup behavior for replaced/deleted profile media.

Done means:

- Profile media is not stored in the database, is not publicly writable, and is linked to the correct bounded context.

### Phase G.2 - External Verification Provider Abstractions

Goal: regulated identity, tax, licensing, credit, and AML checks can be added through provider-specific adapters while the base template keeps a stable integration model.

Examples for downstream Kenya/SACCO solutions:

- IPRS for national ID and passport validation.
- KRA iTax or successor KRA service/API for PIN and tax-certificate verification.
- NTSA for driving licence validation where needed.
- TransUnion, Metropol, or CreditInfo for CRB checks.
- SWIFT Compliance Analytics, Dow Jones, or equivalent providers for AML and sanctions screening.

Base-template design direction:

- Do not implement country- or SACCO-specific provider integrations in BaseTemplate core.
- Define the extension pattern: provider-specific Infrastructure adapters behind Application abstractions.
- Keep provider SDKs and raw API clients out of Domain, UI, and feature handlers.
- Require resilience policies, timeout/circuit-breaker behavior, idempotency keys, secure PII handling, and provider-specific health checks for any downstream implementation.
- Document example abstractions such as `IIdentityVerificationService`, `ITaxPinVerificationService`, `ICreditReferenceService`, and `IAmlScreeningService`.

Done means:

- A downstream solution can add verification providers without coupling domain code to external APIs.

### Phase H - Background Jobs

Goal: recurring and deferred jobs are production-ready.

Current state:

- Quartz is currently used.
- `PLAN.md` previously said Hangfire, which was stale.

Decision needed:

- Keep Quartz if scheduling flexibility and code-first jobs matter most.
- Use Hangfire if dashboard-first operations at `/hangfire` is a hard requirement.
- Do not keep both unless there is a clear split of responsibilities.

Tasks if Quartz stays:

- Add recurring job examples.
- Add persistent store configuration.
- Add job health checks.
- Add job execution logging and tracing.
- Add admin visibility through custom page, health endpoint, logs, or third-party dashboard.

Done means:

- Deferred and recurring jobs can be scheduled, monitored, retried, and diagnosed.

### Phase I - Messaging And Integration Events

Goal: messaging works locally and in Azure.

Tasks:

- Confirm RabbitMQ dev configuration.
- Confirm Azure Service Bus production configuration.
- Ensure consumers are feature-owned.
- Ensure retry, dead-letter, failure logging, and failed-message persistence are consistent.
- Add integration tests for representative consumers.
- Document event versioning rules.

Done means:

- Same consumer code can run with RabbitMQ locally and Azure Service Bus in production through configuration.

### Phase J - Docker And Local Developer Platform

Goal: a developer can run dependencies locally in one command.

Tasks:

- Add Dockerfiles where needed.
- Add Docker Compose for SQL Server, Redis, RabbitMQ, Seq, and optionally Azurite.
- Add `.env.example`.
- Add local setup documentation.
- Ensure appsettings align with Compose service names.

Done means:

- A new developer can run the platform locally without manually installing every dependency.

### Phase K - Frontend, RCL, And MudBlazor

Goal: the web UI is a real consumer of the architecture.

Reference:

- `https://github.com/odhiamboally/ClientOnboarding` may be used as a UI/product reference for customer onboarding ideas.
- Borrow patterns, not old architecture.
- Adapt all naming from Client to Customer.
- Move reusable UI pieces into the RCL where they are feature-neutral or shared across Web and MAUI.
- Keep feature-owned pages/components under the same bounded-context feature convention used elsewhere.

Tasks:

- Decide Blazor auth strategy: cookie, JWT, or hybrid.
- Add MudBlazor to the UI layer.
- Define custom theme in shared UI.
- Build auth pages.
- Build employee pages.
- Build customer pages.
- Build dashboard.
- Build navigation/menu visibility from the authenticated user's allowed modules, roles, permissions, and enabled feature flags.
- Keep static admin/menu definitions only as an interim shell until the dynamic menu model is implemented.
- Adapt useful customer onboarding patterns from the reference repo: searchable customer list, advanced filters, cursor pagination, customer form modal, tabbed onboarding sections, director management, dashboard KPI cards, status breakdowns, aging card, and RM workload card.
- Add feature-flag-aware UI helpers.
- Ensure RCL remains component-focused and does not violate Application coupling rules.

Done means:

- The Blazor app can authenticate, navigate by role/context, and use real backend features.

### Phase K.1 - Dynamic Menus And Admin Surface

Goal: navigation is data-driven and permission-aware without turning the UI into a hard-coded demo shell.

Current interim state:

- Admin Center modules are defined statically in the Blazor UI.
- This is acceptable while the admin surface is still being shaped.
- It is not the final model for tenant-specific, user-configurable, or plugin-driven systems.

Tasks:

- Add a menu/module catalog model owned by IAM or Shared Administration.
- Decide whether module definitions belong in IAM, Shared, or a dedicated Administration feature.
- Store module key, display name, route, icon key, sort order, parent/child relationship, required permission, required feature flag, and enabled state.
- Support nested menu items.
- Load allowed menus for the signed-in user through an API endpoint.
- Filter menus by role, direct user permission grants, contextual claims, feature flags, and tenant where applicable.
- Cache menu responses safely and invalidate when roles, permissions, feature flags, or module configuration changes.
- Keep route authorization enforced server-side; never rely on hidden menus as security.
- Add admin UI for managing modules only after the model is stable.

Done means:

- The sidebar and Admin Center are composed from backend-authorized menu/module data.
- Users only see modules they are allowed to access.
- Direct URL access remains protected by endpoint and policy checks.

### Phase L - MAUI Mobile

Goal: MAUI is a real API client, not just a scaffold.

Tasks:

- Add API client.
- Add token storage.
- Add auth flow.
- Add resilience for mobile API calls.
- Add customer/employee screens as appropriate.
- Keep MAUI from directly referencing Application or Domain.

Done means:

- Mobile can authenticate and consume API endpoints through the same contracts.

### Phase M - SignalR

Goal: real-time capability exists before features need it.

Tasks:

- Add SignalR service registration.
- Add base hub.
- Add authorization for hubs.
- Add notification group strategy by user, role, tenant, and context.
- Add sample event such as dashboard refresh or notification delivery.

Done means:

- A feature can publish real-time updates without inventing infrastructure.

### Phase N - Reporting With QuestPDF

Goal: reports are generated through a reusable reporting abstraction.

Tasks:

- Add reporting contracts.
- Add QuestPDF implementation.
- Add sample report.
- Add API endpoint for report generation.
- Add storage/download strategy.
- Confirm licensing constraints for intended use.

Done means:

- A feature can generate a PDF report through a common abstraction.

### Phase O - Payments

Goal: payment integrations are pluggable.

Tasks:

- Add `IPaymentGateway` abstraction.
- Add provider selection by configuration.
- Add Mpesa implementation.
- Add Stripe implementation.
- Add payment request, callback/webhook, reconciliation, idempotency, and audit model.
- Add tests with mocked providers.
- Add documentation for private GitHub Packages NuGet usage if Mpesa package is private.

Done means:

- Payment providers can be swapped or added without changing feature code.

### Phase P - Reference Business Flow Boundary

Goal: keep only enough business flow in the template to prove the architecture, without turning BaseTemplate into a specific product.

Current rule:

- Customer and Employee/Workforce flows may remain as reference vertical slices because they prove UI-to-API-to-Application-to-Persistence, IAM linkage, validation, caching, soft delete, and admin conventions.
- Complete the existing reference flows to production-quality standards where already introduced.
- Do not add product modules such as SACCO loans, CRB checks, KYC workflows, payroll, leave, or core banking into BaseTemplate core.
- Put solution-specific modules in downstream applications cloned from BaseTemplate, for example Bumure/SACCO.

Recommended downstream modules:

- SACCO/member onboarding
- loans
- KYC/CRB/AML checks
- savings/deposits
- guarantors
- approvals/workflows
- repayments/arrears/servicing
- payroll or full HR, if required

Build-vs-buy guidance:

- Build reusable plumbing once in BaseTemplate: IAM, authorization, audit, logging, validation, integration abstractions, background jobs, health checks, observability, storage, feature flags, CI/CD, deployment.
- Buy or integrate commodity/product systems where appropriate: payroll, accounting, CRM, SMS/email providers, payment gateways, identity federation, credit bureau providers, reporting/BI, and document storage.
- Build custom domain features where the client differentiates or where local workflow rules are specific.

Done means:

- BaseTemplate remains cloneable and renameable for unrelated applications while still containing complete reference flows that demonstrate the standards.

### Phase Q - Azure Deployment, APIM, And Monitoring

Goal: the template can go from local development to Azure safely.

Tasks:

- Add Azure App Service deployment workflow.
- Add deployment environments.
- Add deployment slots if available.
- Add Key Vault integration for production secrets.
- Add Azure Cache for Redis settings.
- Add Azure SQL connection strategy.
- Add Azure Service Bus production messaging configuration.
- Add Azure Monitor and Application Insights configuration.
- Add API Management onboarding plan.
- Add smoke tests after deployment.
- Add rollback guidance.

Done means:

- `main` can produce a deployable artifact and deploy through a documented, repeatable Azure path.

---

## 7. Maturity Tasks

Maturity tasks are deliberate final-hardening or configurability tasks. They are not a place to hide incomplete core behavior.

Rules:

- Core security, correctness, data integrity, and working user flows must not be deferred here.
- Items belong here only when the current implementation is production-safe but not yet maximally configurable.
- Each maturity item must say what trigger makes it necessary.
- Revisit this section before declaring a phase complete.

Current maturity tasks:

- Move any remaining static demo dashboard/status content to backend read models when those panels become operational.
- Add plugin-driven module discovery if the template evolves into installable vertical modules.
- Review whether endpoint URL configuration should remain appsettings-based or move to a central service-discovery/configuration provider once multiple deployable services exist.

---

## 8. Suggested Execution Order

Use this order to close the current phase quickly while still moving toward the full enterprise base template.

1. Revise and commit this plan.
2. Make the API run locally from Visual Studio with local SQL Server and Seq.
3. Create initial EF migrations and development seed/admin path.
4. Finish API and IAM completeness.
5. Complete exception handling, validation, and pipeline behaviours.
6. Complete RBAC/ABAC/permission-based authorization and policy enforcement.
7. Complete health checks, audit, observability, and security checklist.
8. Add feature flags.
9. Complete frontend auth and MudBlazor implementation.
10. Add dynamic menu loading once authorization and feature flags are stable enough to drive visibility.
11. Add Docker Compose local platform.
12. Add Azure deployment workflow.
13. Add Entra ID SSO.
14. Add SignalR, reporting, payment abstractions, and other reusable platform capabilities.

---

## 9. Open Decisions

These decisions should be made deliberately before implementation:

- Quartz vs Hangfire for background jobs and dashboard expectations.
- Cookie auth vs JWT-only vs hybrid auth for Blazor.
- Audit model: columns only, audit table, event stream, or hybrid.
- Menu/module catalog ownership: IAM, Shared Administration, or dedicated Administration feature.
- Permission model granularity: role permissions only, direct user grants, contextual ABAC rules, or hybrid.
- Payment module scope for the base template: abstraction only or reusable provider implementations.
- Which reference business flows remain in BaseTemplate versus move to downstream starter modules.
- How much of Azure APIM should be automated immediately versus documented first.
- Whether to migrate away from MediatR later.

---

## 10. Session Onboarding

At the start of a new work session:

1. Read this file.
2. Check `git status`.
3. Check recent commits and merged PRs.
4. Check architecture tests for enforced rules.
5. Continue from the current execution order.

At the end of a work session:

1. Update this file if scope or status changed.
2. Run relevant local checks where practical.
3. Generate a concise commit message.
4. Push through a feature branch and PR, not directly to `main`.
