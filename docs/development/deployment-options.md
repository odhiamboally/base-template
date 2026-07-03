# Deployment Options

This document explains the supported deployment paths for BaseTemplate and cloned products. It keeps Azure-ready work intact while allowing local-first and non-Azure deployment when Azure credits or subscriptions are unavailable.

## Principles

- Deployment target is a CI/CD and environment choice, not an application feature flag.
- Runtime providers are selected through typed configuration: messaging transport, cache provider, storage provider, email provider, and payment provider.
- Azure configuration stays committed and documented even when Azure deployment is disabled.
- Local development uses Visual Studio for app hosts and Docker Compose for replaceable infrastructure.
- Production deployments should build immutable artifacts, run migrations as a release step, and smoke-test the deployed application.

## Supported Deployment Models

| Model | Flow | Status | Best Use |
| --- | --- | --- | --- |
| Azure DevOps to Azure Container Apps | Visual Studio -> Azure Repos -> Azure Pipelines -> Docker image -> ACR or Docker Hub -> Azure Container Apps | Planned | Azure-first clients who standardize on Azure DevOps |
| GitHub Actions to Azure Container Apps or App Service | Visual Studio -> GitHub -> GitHub Actions -> Docker image or app artifact -> ACR/GHCR/Docker Hub -> Azure | Partly wired; gated by `AZURE_DEPLOYMENT_ENABLED` | Recommended Azure path once subscription is active |
| GitHub Actions to non-Azure container host | Visual Studio -> GitHub -> GitHub Actions -> Docker image -> GHCR/Docker Hub -> Heroku, DigitalOcean, or VPS | Planned | When Azure is unavailable or a client chooses another host |
| Local Docker deployment | Visual Studio or CLI -> local Docker image -> local Docker Compose runtime | Planned | Local demos, smoke testing, and offline proof-of-concept work |

## Current Recommendation

Use this sequence until Azure credits or a paid subscription are available:

1. Keep Azure deployment workflows and settings in the repository.
2. Leave `AZURE_DEPLOYMENT_ENABLED=false` or unset.
3. Run the app from Visual Studio with local Docker infrastructure.
4. Add a provider-neutral container publish workflow that can push API/UI images to GHCR or Docker Hub.
5. Add a local app-compose profile only after Dockerfiles are finalized.
6. Add DigitalOcean or Heroku deployment only when we choose a specific host to certify.

This lets us keep moving without throwing away the Azure production path.

## Azure Paths

### Azure DevOps To Azure Container Apps

This path is viable when a client uses Azure Repos and Azure Pipelines.

Required pieces:

- Azure Container Registry or Docker Hub/GHCR credentials.
- Azure Container Apps environment.
- Azure SQL or another supported production database.
- Azure Key Vault.
- Azure Blob Storage.
- Azure Service Bus for production messaging, or RabbitMQ if the client deliberately hosts RabbitMQ.
- Azure Redis or another distributed cache.
- Pipeline service connection or workload identity.

Use this model for enterprise Azure clients who want all governance inside Azure DevOps.

### GitHub Actions To Azure

This is the current preferred Azure path for this repository.

The existing deployment workflow is intentionally gated:

- `AZURE_DEPLOYMENT_ENABLED` missing, empty, or `false`: CI can run, deployment is skipped.
- `AZURE_DEPLOYMENT_ENABLED=true`: the deployment job may build, migrate, and deploy to Azure after the required secrets, variables, environments, and Azure permissions are ready.

This workflow should remain in the repo even while Azure is unavailable. It becomes active when the subscription and cost setup are ready.

## Non-Azure Container Hosts

### DigitalOcean

DigitalOcean App Platform can deploy applications from container images and supports DigitalOcean Container Registry, Docker Hub, and GitHub Container Registry. Its container-image path is a good fit if we want a managed host without Azure. DigitalOcean also supports Docker-based VPS deployments through Droplets when we want more control.

Important fit notes:

- Prefer Linux AMD64 container images.
- Use App Platform environment variables for configuration.
- Use managed Postgres/Redis only if the cloned application can use them. BaseTemplate currently uses SQL Server, so a SQL Server-compatible external database or a VPS-based SQL Server plan must be decided separately.
- Use RabbitMQ if the app needs the same broker model outside Azure.

Reference: [DigitalOcean App Platform container image deployment](https://docs.digitalocean.com/products/app-platform/how-to/deploy-from-container-images/).

### Heroku

Heroku supports container deployment through Heroku Container Registry and Runtime. It is useful for smaller demos or simple production workloads, but it has platform constraints that matter for this template.

Important fit notes:

- The app must listen on Heroku's assigned `$PORT`.
- Dyno filesystem is ephemeral, so profile images and Data Protection keys must use external storage.
- Containers should be built for `linux/amd64`.
- Heroku recommends buildpacks unless a custom Docker image is specifically needed.
- SQL Server is not a natural Heroku add-on path, so database hosting must be decided separately.

Reference: [Heroku Container Registry and Runtime](https://devcenter.heroku.com/articles/container-registry-and-runtime).

### VPS Or Generic Docker Host

This path is:

```text
Visual Studio -> GitHub -> GitHub Actions -> Docker image -> GHCR/Docker Hub -> VPS Docker Compose
```

It is the most flexible non-Azure option. It also requires the most operational ownership:

- TLS and reverse proxy.
- Backups.
- OS patching.
- Container restarts and monitoring.
- Secret storage.
- Database operations.
- RabbitMQ and Redis operations.

This is acceptable for learning and some client budgets, but it is less managed than Azure, DigitalOcean App Platform, or Heroku.

## Local Docker Deployment

Local Docker deployment is different from the current local platform.

Current local platform:

- Runs infrastructure only: RabbitMQ, Redis, Mailpit, Azurite, optional Seq, optional SQL Server.
- Runs `BT.Api` and `BT.UI.Blazor` from Visual Studio.

Future local app deployment:

- Builds API/UI Docker images.
- Runs API/UI plus infrastructure through Compose.
- Uses the same local secrets and `.env` conventions.

This is useful for demoing a deployable system locally, but Visual Studio remains the preferred development/debugging path.

## Switching Rules

Use configuration for runtime providers:

| Concern | Local default | Azure default | Non-Azure default |
| --- | --- | --- | --- |
| Messaging | RabbitMQ | Azure Service Bus | RabbitMQ |
| Cache | Redis container | Azure Redis | Redis |
| Email | Mailpit | Approved email API provider | Approved email API provider |
| Profile images | Azurite or local filesystem | Private Azure Blob | Host-specific object storage or private volume |
| Data Protection | Local key ring | Azure Blob plus Key Vault | External persistent key ring |
| SQL | Local SQL Server | Azure SQL | SQL Server-compatible host |

Use CI/CD variables for deployment target:

```text
DEPLOY_TARGET=none
DEPLOY_TARGET=azure-app-service
DEPLOY_TARGET=azure-container-apps
DEPLOY_TARGET=digitalocean-app-platform
DEPLOY_TARGET=heroku-container
DEPLOY_TARGET=generic-docker
```

Do not use application feature flags to choose deployment platform. Feature flags control product behavior at runtime. Deployment variables control where CI/CD sends artifacts.

## Messaging Decision

RabbitMQ is the correct local and non-Azure transport for now. Azure Service Bus remains the Azure production transport. The same MassTransit consumers should work across both, with differences isolated in configuration and transport registration.

## Payment Provider Setup

Payment provider configuration is separate from deployment platform. A cloned application can use Stripe, M-Pesa, both, or neither.

### Stripe

From Stripe, collect:

- Test mode secret key, such as `sk_test_...`.
- Publishable key, such as `pk_test_...`, if frontend checkout uses Stripe-hosted UI or Stripe.js.
- Webhook signing secret, such as `whsec_...`, when enabling webhook callbacks.
- Success and cancel URLs.
- Enabled payment methods and currency rules.

Store values in user-secrets locally and Key Vault or host secrets in production. Never commit them.

### M-Pesa Daraja

From the M-Pesa developer portal, collect:

- Consumer key.
- Consumer secret.
- Sandbox or production base URL.
- ShortCode, PayBill, or Till number.
- STK Push PassKey.
- Callback URL reachable by Safaricom.
- Initiator name and security credential for flows that need them.

For local callback testing, use a tunnel such as ngrok or Cloudflare Tunnel. The callback URL cannot be only `localhost` because Safaricom must call it from outside your machine.

## What Happens On Commit And PR

When Azure is disabled:

- Backend, frontend, mobile, full-solution, and architecture workflows can still run.
- Azure deployment jobs should skip because `AZURE_DEPLOYMENT_ENABLED` is not `true`.
- Container publish jobs, once added, can still build and publish images to GHCR or Docker Hub without deploying to Azure.

When Azure is enabled:

- The deployment workflow may run migrations and deploy according to the protected GitHub environment.
- Azure failures should be treated as deployment-environment failures, not local-development failures.

## Next Implementation Steps

1. Add production Dockerfiles for API and Blazor UI.
2. Add a container-publish workflow that pushes to GHCR by default.
3. Add `DEPLOY_TARGET` handling for manual dispatch.
4. Add local app Docker Compose using the published or locally built images.
5. Add DigitalOcean or Heroku workflow only after we choose the host to certify.
6. Keep Azure workflow disabled until subscription billing is active again.
