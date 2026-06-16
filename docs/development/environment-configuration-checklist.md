# Environment Configuration Checklist

This checklist explains how BaseTemplate should be configured locally, when developing locally against Azure services, and when deployed to Azure.

## Configuration Modes

BaseTemplate uses `ASPNETCORE_ENVIRONMENT` for application behavior and normal .NET configuration precedence for values.

- `Development`: local developer experience. Non-secret defaults live in `appsettings.Development.json`; developer-specific secrets live in user-secrets.
- `Production`: deployed runtime behavior. Secrets come from App Service settings, Key Vault references, Key Vault configuration provider, or managed identity.
- Azure-backed local development: still uses `Development`, but selected services point at Azure through user-secrets. This is not a separate environment name; it is a configuration choice.

Avoid creating a custom environment name such as `Native` unless we have a clear hosting reason. It is simpler and safer to keep `Development` plus explicit Azure-backed settings.

## Where Values Belong

- `appsettings.json`: safe defaults, placeholders, feature structure, and non-secret values.
- `appsettings.Development.json`: safe local defaults and local-only toggles.
- User-secrets: local developer secrets such as SQL connection strings, SMTP credentials, JWT secret, Key Vault URI, and Azure storage connection strings.
- Environment variables: CI/CD, containers, App Service settings, and temporary overrides.
- Azure Key Vault: production secrets and managed secrets used by deployed apps.
- Azure App Service settings: runtime settings, Key Vault references, and non-secret deployment configuration.

Never commit real passwords, account keys, connection strings, API keys, or temporary provisioning passwords.

## Naming Rules

- .NET configuration sections use `:` locally, for example `DataProtection:BlobKeyUri`.
- App Service environment variables use `__`, for example `DataProtection__BlobKeyUri`.
- Key Vault secret names use `--`, for example `DataProtection--BlobKeyUri`.

## Azure Services Used By The Template

| Concern | Azure service | Primary configuration keys | Status |
| --- | --- | --- | --- |
| SQL database | Azure SQL or SQL Server | `ConnectionStrings:DefaultConnection` | Required for API runtime |
| Data Protection key ring | Azure Blob Storage | `DataProtection:BlobKeyUri` | Implemented |
| Data Protection key encryption | Azure Key Vault key | `DataProtection:KeyEncryptionMode`, `DataProtection:KeyVaultKeyIdentifier` | Implemented |
| Profile images | Azure Blob Storage | `ProfileImageStorage:Provider`, `ProfileImageStorage:AzureBlob:ContainerUri` | Implemented |
| Secret loading | Azure Key Vault | `KeyVault:Uri` | Implemented |
| App hosting | Azure App Service | App settings and managed identity | Ready for deployment work |
| Observability | Application Insights / Azure Monitor | `Observability:AzureMonitor:ConnectionString`, `ApplicationInsights:ConnectionString`, or `APPLICATIONINSIGHTS_CONNECTION_STRING` | Implemented with fallbacks |
| Messaging | Azure Service Bus | `Messaging:Transport`, `Messaging:AzureServiceBus:ConnectionString` | Implemented, needs production smoke test |
| Caching | Azure Cache for Redis | `CacheSettings:Azure:ConnectionString` | Config shape exists, needs full production smoke test |

## Recommended Local Setup

Use user-secrets for local development:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<local-sql-connection>" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "JwtSettings:Secret" "<strong-local-jwt-secret>" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "EmailSettings:Username" "<smtp-username>" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "EmailSettings:Password" "<smtp-password-or-app-password>" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "IamProvisioning:TemporaryPassword" "<temporary-password>" --project src\Backend\Api\BT.Api\BT.Api.csproj
```

For local-only development, leave these values empty so the app uses filesystem/local storage:

```powershell
dotnet user-secrets remove "DataProtection:BlobKeyUri" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets remove "DataProtection:KeyVaultKeyIdentifier" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "ProfileImageStorage:Provider" "Local" --project src\Backend\Api\BT.Api\BT.Api.csproj
```

## Recommended Azure-Backed Local Setup

Use this when testing Azure integrations from your machine. Your Visual Studio or Azure CLI identity must have the required Azure RBAC roles.

```powershell
dotnet user-secrets set "DataProtection:KeyEncryptionMode" "KeyVault" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "DataProtection:BlobKeyUri" "https://<storage-account>.blob.core.windows.net/dataprotection-keys/keyring.xml" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "DataProtection:KeyVaultKeyIdentifier" "https://<vault>.vault.azure.net/keys/<key>/<version>" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "ProfileImageStorage:Provider" "AzureBlob" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "ProfileImageStorage:AzureBlob:ContainerUri" "https://<storage-account>.blob.core.windows.net/profile-images" --project src\Backend\Api\BT.Api\BT.Api.csproj
```

If your local Azure identity is not ready, use `ProfileImageStorage:AzureBlob:ConnectionString` and `ProfileImageStorage:AzureBlob:ContainerName` only as a controlled local fallback.

## Recommended Azure Production Setup

In Azure App Service:

```text
ASPNETCORE_ENVIRONMENT=Production
KeyVault__Uri=https://<vault>.vault.azure.net/
DataProtection__KeyEncryptionMode=KeyVault
DataProtection__BlobKeyUri=https://<storage-account>.blob.core.windows.net/dataprotection-keys/keyring.xml
DataProtection__KeyVaultKeyIdentifier=https://<vault>.vault.azure.net/keys/<key>/<version>
ProfileImageStorage__Provider=AzureBlob
ProfileImageStorage__AzureBlob__ContainerUri=https://<storage-account>.blob.core.windows.net/profile-images
ProfileImageStorage__AzureBlob__BlobPrefix=profile-images
```

Optional observability keys:

```text
APPLICATIONINSIGHTS_CONNECTION_STRING=<application-insights-connection-string>
```

or Key Vault:

```text
ApplicationInsights--ConnectionString=<application-insights-connection-string>
```

## Azure RBAC Checklist

Grant these roles before testing Azure-backed local development or App Service deployment:

- Developer user or App Service managed identity: `Storage Blob Data Contributor` on the storage account or required containers.
- Developer user or App Service managed identity: `Key Vault Crypto User` on the Data Protection key.
- Developer user who creates keys: `Key Vault Crypto Officer` or `Key Vault Administrator`.
- App Service: system-assigned managed identity enabled.

## Current Configuration Review Rules

When checking screenshots or app settings, verify:

- `DataProtection__BlobKeyUri` points to the `dataprotection-keys` container and a blob name such as `keyring.xml`.
- `DataProtection__KeyEncryptionMode` is `KeyVault` or `Auto` for Azure.
- `DataProtection__KeyVaultKeyIdentifier` points to a Key Vault key version, not a secret or certificate.
- `DataProtection__CertificateThumbprint` is empty unless deliberately using certificate mode.
- `ProfileImageStorage__Provider` is `AzureBlob` when testing Azure profile image uploads.
- `ProfileImageStorage__AzureBlob__ContainerUri` points to the private `profile-images` container.
- `ConnectionStrings--DefaultConnection` exists in Key Vault or `ConnectionStrings__DefaultConnection` exists in App Service.
- `JwtSettings--Secret` or `JwtSettings--SecretKey` exists in Key Vault.
- Email credentials exist and `EmailSettings:FromAddress` is allowed by the SMTP provider.

## Troubleshooting

- If sign-in shows “The identity service is unavailable,” first confirm the API is running and the UI `BackendApi:BaseUrl` points at the API URL.
- If sign-in fails with a Data Protection `403`, the identity used by the app lacks `Storage Blob Data Contributor` for the key-ring blob/container.
- If Data Protection fails with Key Vault authorization errors, the identity lacks `Key Vault Crypto User` for the key.
- If profile upload succeeds visually but no blob appears, confirm the backend response succeeded, `ProfileImageStorage:Provider=AzureBlob`, and the current user has completed required MFA enrollment.
- If old package errors mention `FluentEmail.MailKit`, clean stale build output and rebuild; source code should no longer reference FluentEmail.
