# SaaS Multitenancy Strategy

This document defines how BaseTemplate should model SaaS tenancy, deployment isolation, and environment configuration before cloning the template for client products.

## Decision

BaseTemplate uses a hybrid deployment-stamp SaaS model.

- A tenant is a customer organization, business unit, or client workspace served by a product built from BaseTemplate.
- A deployment stamp is an isolated runtime and resource boundary. A stamp can host one tenant or a controlled pool of tenants.
- The same Docker image and codebase can be deployed many times with different stamp-scoped configuration.
- Resource groups, Key Vaults, databases, storage accounts, caches, and messaging resources are provisioned by infrastructure code, not by copying portal settings manually.

This gives us a low-cost pooled path for early tenants and an isolated premium/compliance path for clients that need stronger separation.

## Supported Isolation Tiers

| Tier | Compute | Database | Key Vault | Storage | Cache and messaging | Use case |
| --- | --- | --- | --- | --- | --- | --- |
| Local development | Visual Studio app hosts plus Docker infrastructure | Local SQL Server | User-secrets and local key ring | Local filesystem or Azurite | Redis, RabbitMQ, Mailpit, Azurite containers | Development and demos |
| Pooled production stamp | Shared container app or app service for a tenant pool | Shared database with tenant filters | Stamp Key Vault | Stamp storage account | Stamp Redis and Service Bus/RabbitMQ | Low-cost tenants with acceptable logical isolation |
| Isolated tenant stamp | Tenant-specific container app or app service | Tenant database or isolated elastic-pool database | Tenant or stamp Key Vault | Tenant or stamp storage account | Tenant or stamp cache and broker | Regulated, premium, noisy-neighbor-sensitive, or custom clients |

Pooled stamps are allowed only when tenant isolation is enforced at the data layer and covered by tests. Isolated stamps are preferred when a client needs physical isolation, custom resources, region-specific hosting, or stricter compliance boundaries.

## Control Plane And Data Plane

BaseTemplate should separate SaaS control-plane concerns from product data-plane concerns.

The control plane owns tenant and deployment metadata:

- Tenant ID, display name, hostnames, status, and isolation tier.
- Deployment stamp ID and target resource group.
- Database provider and connection resolution strategy.
- Key Vault URI and storage/cache/messaging provider choices.
- Feature flags, enabled integrations, and optional add-ons.

The data plane serves normal product traffic:

- Resolves the current tenant from hostname, authenticated claims, or an approved tenant resolver.
- Applies tenant filters and authorization to business data.
- Reads only the stamp-scoped configuration injected into the host.
- Does not know about unrelated tenant resource groups, Key Vaults, or databases.

## Configuration Rules

The application code reads stable configuration sections. The deployment stamp supplies the values.

- Code reads normal .NET keys such as `ConnectionStrings:DefaultConnection`, `CacheSettings:Provider`, and `ProfileImageStorage:Provider`.
- Local development uses `dotnet user-secrets`.
- Azure App Service and Azure Container Apps use environment variables and Key Vault references.
- Key Vault secret names use double hyphen mapping, for example `CacheSettings--Azure--ConnectionString`.
- Environment variables use double underscore mapping, for example `CacheSettings__Azure__ConnectionString`.
- Each isolated stamp can reuse simple secret names because the Key Vault itself is isolated.
- Avoid one shared Key Vault with tenant-prefixed secrets unless we deliberately choose a shared-compute model and have tests proving tenant secret routing cannot cross boundaries.
- Managed identity access must be scoped to the stamp resources the app is allowed to read.

For Azure Container Apps, Bicep or Terraform should inject the stamp-specific environment variables and secret references. The container image must not contain secrets.

## Infrastructure As Code Rules

Do not manually copy resource groups in the portal. Define reusable infrastructure modules and parameter files.

Core modules should cover:

- Resource group or stamp boundary.
- Container registry selection: GHCR, ACR, or another approved registry.
- Container Apps environment or App Service plan.
- API and UI container apps or web apps.
- Managed identities and role assignments.
- Key Vault.
- SQL Server/Azure SQL, with conditional support for other database providers only when the code and migrations are certified for them.
- Storage account and private containers.
- Redis.
- Service Bus or selected broker.
- Application Insights or approved observability sink.

Optional add-on modules should be conditional, for example AI Search, extra storage, per-tenant broker, or a different database provider for a specific client.

## Stamp Authentication Policy

- **Pooled stamps** stay single-app, meaning the Control Plane and Data Plane share the same overarching identity provider configuration (e.g., a shared Entra ID app registration).
- **Isolated and regulated stamps** MUST get a separate ControlPlane auth realm. This requires a distinct Entra ID app registration (or equivalent IdP configuration) per isolated stamp, with a distinct JWT audience.
- Platform staff must structurally never receive a `tenant_id` claim within the isolated realm, ensuring complete structural isolation of identity and preventing token reuse across boundaries.

## Database Strategy

BaseTemplate currently supports the pooled-database model through tenant-scoped entities and global query filters.

Before marking database-per-tenant or PostgreSQL support as production-certified, we must add:

- A tenant catalog and connection resolver.
- Provider-specific EF Core registration.
- Provider-specific migration bundle strategy.
- Automated tests for tenant isolation and migration execution.
- Operational runbooks for backup, restore, and tenant movement.

Do not claim a provider is supported just because infrastructure can provision it. The code, migrations, tests, and deployment workflow must be certified together.

## Deployment Flows

The intended container flow is:

```text
Visual Studio -> GitHub Repo -> GitHub Actions -> Docker image -> GHCR or ACR -> Azure Container Apps
```

The same image can be deployed to multiple stamps:

```text
BaseTemplate API image:v1.2.0 -> aca-tenant-a with Tenant A configuration
BaseTemplate API image:v1.2.0 -> aca-tenant-b with Tenant B configuration
BaseTemplate API image:v1.2.0 -> aca-custom-client with custom provider configuration
```

The registry and deployment target are CI/CD decisions. Runtime provider behavior is configuration. Product behavior is feature flags. Keep these concerns separate.

## Blazor And API Boundary

Blazor remains an API client in this template. It must not own database credentials, Key Vault access, or tenant data isolation policy.

Real-time dashboards should use API-authorized SignalR channels and tenant-aware groups. Do not make dashboard refreshes bypass the API or tenant policy.

## Readiness Gate

SaaS multitenancy is ready when:

- The current tenant can be resolved consistently.
- Tenant-scoped data cannot leak across tenants.
- Stamp-scoped configuration is documented and tested.
- IaC can provision at least one pooled stamp and one isolated stamp shape.
- CI/CD can deploy the same image with different stamp configuration.
- Key Vault, storage, cache, messaging, and database settings are aligned with the selected stamp.

---

## Middleware Ordering And Tenant Resolution Contract

This section documents a deliberate architectural decision. **Do not change the middleware order without updating this section.**

`TenantResolutionMiddleware` is placed in the pipeline **after `UseRouting()`** and **before `UseAuthentication()`**:

```
UseRouting()
UseMiddleware<TenantResolutionMiddleware>()   ← hostname-based resolution runs here
UseAuthentication()                            ← JWT tenant_id claim is available from here
UseAuthorization()
```

**Why before `UseAuthentication()`?**
Hostname resolution does not require an authenticated user. The middleware resolves the tenant purely from the `Host` HTTP header (or `X-Forwarded-Host` for proxied requests), looks up the matching `Tenant.HostName` in the ControlPlane database (cached for 5 minutes), and injects the resolved tenant ID into a request header.

**Resolution fallback priority in `CurrentTenantProvider`:**
1. `tenant_id` JWT claim (populated post-auth for already-authenticated requests).
2. Request header injected by `TenantResolutionMiddleware` (hostname-derived).
3. `OrgSettings:DefaultTenantId` configuration value (single-stamp deployments).
4. Throws `TenantNotResolvedException` if none of the above resolves.

**Claim vs. header precedence:** JWT claim wins. For isolated stamps, the JWT `tenant_id` claim is the authoritative source. The header is a fallback for anonymous or pre-auth paths.

**Security implication:** If a proxied request arrives with a forged `X-Forwarded-Host`, it could influence hostname-based resolution. Mitigate this by configuring `ForwardedHeadersOptions.KnownProxies` or `KnownNetworks` to only accept forwarded headers from trusted upstream addresses.

---

## Tenant Lifecycle States

| Status | Meaning | Expected Operator Action |
|---|---|---|
| `Active` | Fully operational. Tenant traffic is served. | None. |
| `Provisioning` | Tenant record created; isolated-stamp infrastructure is being deployed via GitHub Actions. | Wait for the provisioning workflow to complete, then call `POST /api/v1/control-plane/tenants/{id}/activate`. |
| `ProvisioningFailed` | The GitHub Actions workflow dispatch failed. Infrastructure may not exist. | Diagnose the failure via GitHub Actions logs. Fix the configuration, then re-trigger via `POST /api/v1/control-plane/tenants/{id}/activate` (which will retry provisioning). |
| `Suspended` | Tenant is temporarily blocked. Requests for this tenant will not be served. | Call `POST /api/v1/control-plane/tenants/{id}/activate` to restore access. |

> [!IMPORTANT]
> A tenant in `Provisioning` or `ProvisioningFailed` status will **not** be resolved by `TenantResolutionMiddleware`. The middleware filters on `TenantStatus.Active` only. This prevents premature data access before the isolated database is ready.

