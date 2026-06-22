# Local Platform

BaseTemplate runs application code from Visual Studio and runs replaceable infrastructure dependencies in Docker. This preserves normal .NET debugging while making a new developer machine reproducible.

Docker Desktop groups these services under the Compose project name `cg-basetemplate`. The `cg-` prefix identifies the row as a container group rather than an individual service.

Data volumes retain stable `basetemplate_*` names independently of the group label. The setup script creates missing volumes and reuses existing ones, allowing container-group renames and container recreation without abandoning local data.

## Runtime Boundary

Run from Visual Studio:

- `BT.Api`
- `BT.UI.Blazor`
- EF Core migrations and application debugging

Run in Docker:

- RabbitMQ for local MassTransit messaging
- Redis for distributed cache and HybridCache L2
- Mailpit for safe local SMTP capture
- Seq when a locally installed Seq instance is not already available
- Azurite for local Azure Blob-compatible profile-image storage
- SQL Server when a developer does not want to use an installed SQL Server instance

Azure Service Bus is an Azure-managed service and does not run in Docker. The same MassTransit consumers use RabbitMQ locally and Azure Service Bus in Azure by changing `Messaging:Transport` and its provider settings.

## First-Time Setup

Docker Desktop must be running. From the repository root:

```powershell
./scripts/setup-local-platform.ps1
```

The script:

1. Generates random local-only credentials in ignored `ops/local/.env`.
2. Writes matching RabbitMQ, Redis, and Mailpit values to API user-secrets.
3. Pulls and starts RabbitMQ, Redis, Mailpit, and Azurite.
4. Waits for container health checks.

Run the script from the repository root, the folder containing `BaseTemplate.sln`, `AGENTS.md`, and the `scripts` folder:

```powershell
cd E:\Repos\BaseTemplate
./scripts/setup-local-platform.ps1
```

It is a repository script, not a globally installed PowerShell command. You may invoke it from another folder only by supplying its full path.

Optional profiles:

```powershell
# Add Seq when port 5341 is not already used by a local Seq installation.
./scripts/setup-local-platform.ps1 -IncludeSeq

# Add SQL Server on localhost,14333 and update the API connection-string secret.
./scripts/setup-local-platform.ps1 -IncludeSqlServer

# Start every local dependency.
./scripts/setup-local-platform.ps1 -IncludeSqlServer -IncludeSeq
```

Do not use `-IncludeSqlServer` when retaining an existing SQL Server/SSMS database connection. The script leaves the existing `ConnectionStrings:DefaultConnection` secret unchanged unless that switch is supplied.

The setup script is idempotent and can be run whenever Docker services or local user-secrets need to be aligned. It creates credentials only when the ignored `ops/local/.env` file does not exist; later runs reuse those credentials and safely reconcile containers. It is not required before every Visual Studio launch when the containers are already running and the secrets have not changed.

The standard setup selects `ProfileImageStorage:Provider=Azurite`, uses the separate `ProfileImageStorage:Azurite` section with `UseDevelopmentStorage=true`, and targets the private `profile-images` container. Filesystem storage remains available as an explicit fallback by selecting `Provider=Local`. The `ProfileImageStorage:AzureBlob` section remains independent and preserved for Azure production.

## Local Endpoints

| Service | Endpoint | Purpose |
| --- | --- | --- |
| RabbitMQ | `localhost:5672` | AMQP transport |
| RabbitMQ Management | `http://localhost:15672` | Exchanges, queues, consumers, and message rates |
| Redis | `localhost:6380` | HybridCache distributed layer; avoids common existing port `6379` |
| Mailpit | `http://localhost:8025` | Captured email UI |
| Mailpit SMTP | `localhost:1025` | Local SMTP relay |
| Seq | `http://localhost:5341` | Structured logs, optional profile |
| Azurite Blob | `http://localhost:10000` | Storage emulator, optional profile |
| SQL Server | `localhost,14333` | Containerized database, optional profile |

## Operations

```powershell
# Inspect service state.
docker compose --env-file ops/local/.env -f ops/local/docker-compose.yml ps

# Stop containers while preserving data volumes.
./scripts/stop-local-platform.ps1

# Include the same optional profiles that were started.
./scripts/stop-local-platform.ps1 -IncludeSeq

# Restart the core platform.
docker compose --env-file ops/local/.env -f ops/local/docker-compose.yml up -d --wait
```

The repository stop script deliberately exposes no volume-deletion option. It cannot prevent a developer from invoking Docker directly, so do not run `docker compose down -v` unless local RabbitMQ, Redis, SQL, Seq, and Azurite data should be permanently deleted.

## RabbitMQ Outbox Certification

Run the real RabbitMQ round-trip test from the repository root:

```powershell
./scripts/test-local-messaging.ps1
```

The script reads generated RabbitMQ credentials from the ignored local environment file without printing them, ensures RabbitMQ is healthy, and runs the `ExternalRabbitMq` integration test. The test:

1. Starts MassTransit against the Docker RabbitMQ broker.
2. Publishes a uniquely identified event through the EF Core bus outbox.
3. Commits the SQLite transaction containing the outbox record.
4. Waits for MassTransit's outbox delivery service to publish the event.
5. Verifies a real RabbitMQ consumer receives the same event.
6. Verifies the EF outbox record is drained.

The ordinary integration suite skips this external test when Docker credentials are unavailable. This keeps CI deterministic while the dedicated script certifies the complete local transport path.

## Azure Deployment Gate

The Azure deployment workflow remains committed but is gated by the GitHub repository variable `AZURE_DEPLOYMENT_ENABLED`.

- Missing, empty, or `false`: pushes to `main` may register the workflow, but the deployment job is skipped. Normal backend, frontend, mobile, full-solution, and architecture CI remain independent.
- `true`: a qualifying push to `main` automatically builds, tests, creates migration bundles, runs migrations, and deploys the API and UI through the protected `production` environment.

Set it in GitHub under **Settings > Secrets and variables > Actions > Variables > New repository variable**. Use the name `AZURE_DEPLOYMENT_ENABLED` and value `true` only after the Azure subscription, OIDC variables, SQL connection secret, managed identities, and production environment approval are ready.

## Provider Mapping

| Concern | Local | Azure production |
| --- | --- | --- |
| Messaging | RabbitMQ | Azure Service Bus |
| Distributed cache | Redis container | Azure Managed Redis using managed identity |
| SQL | Installed SQL Server or SQL container | Azure SQL |
| Email | Mailpit | Approved SMTP/email provider |
| Logs | Seq | Azure Monitor/Application Insights, optionally Seq |
| Profile images | Local filesystem or Azurite | Private Azure Blob Storage |
| Data Protection | Local key ring | Azure Blob key ring encrypted by Key Vault |

The Azure OIDC workflow, migration bundles, App Service deployment jobs, managed-identity configuration, and Key Vault/Blob settings remain in the repository. Local configuration overrides them through `appsettings.Development.json` and user-secrets; it does not replace them.

Development explicitly disables external Data Protection key storage and SMTP authentication. Existing Azure Blob/Key Vault URIs and real SMTP credentials can remain configured without being contacted by the local runtime.

## Messaging License Decision

BaseTemplate pins MassTransit `8.5.10`, which targets .NET 10 and is published under Apache-2.0. MassTransit v9 is a commercial release and is not used by the reusable template. Revisit this decision deliberately if a future application purchases MassTransit v9 support and features.
