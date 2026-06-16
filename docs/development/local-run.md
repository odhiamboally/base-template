# Local Run Guide

This guide is for running the solution locally from Visual Studio before Docker and Azure deployment are added.

## Current Local Strategy

Use locally installed services first:

- SQL Server through SSMS.
- Seq at `http://localhost:5341`.
- Visual Studio launch profiles for the API and UI.

Docker is intentionally postponed until the application is working locally. Docker will later become the repeatable dependency setup for new machines and CI/CD environments.

## Development Toggles

The Development environment disables infrastructure that should not block basic local API/UI work:

- `Messaging:Enabled = false`
- `BackgroundJobs:Enabled = false`
- `Observability:Enabled = false`
- `DevelopmentSeed:Enabled = true`

Production defaults keep these capabilities enabled unless explicitly configured otherwise.

## Why Messaging Is Disabled Locally

The solution currently references MassTransit v9 packages. MassTransit v9 requires a runtime license. To keep local development moving, Development disables the bus and uses a no-op integration event publisher.

Before production messaging is finalized, decide one of these paths:

- provide a MassTransit v9 license
- downgrade to the latest acceptable MassTransit v8 line
- replace direct MassTransit usage with an internal abstraction over native broker clients

## Required User Secrets

Set local secrets on the API project:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=BT;Trusted_Connection=True;TrustServerCertificate=True;" --project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet user-secrets set "JwtSettings:SecretKey" "LOCAL-DEVELOPMENT-ONLY-CHANGE-ME-TO-A-LONG-RANDOM-SECRET-AT-LEAST-32-CHARS" --project src\Backend\Api\BT.Api\BT.Api.csproj
```

Adjust the SQL Server name if your instance is different, for example:

```powershell
Server=.\SQLEXPRESS;Database=BT;Trusted_Connection=True;TrustServerCertificate=True;
```

## Apply Local Database Schema

Run the EF migrations once after setting the connection string:

```powershell
dotnet ef database update --context IamDBContext --project src\Backend\Persistence\BT.Persistence\BT.Persistence.csproj --startup-project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet ef database update --context HrDBContext --project src\Backend\Persistence\BT.Persistence\BT.Persistence.csproj --startup-project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet ef database update --context SharedDBContext --project src\Backend\Persistence\BT.Persistence\BT.Persistence.csproj --startup-project src\Backend\Api\BT.Api\BT.Api.csproj
dotnet ef database update --context BankingDBContext --project src\Backend\Persistence\BT.Persistence\BT.Persistence.csproj --startup-project src\Backend\Api\BT.Api\BT.Api.csproj
```

The bounded contexts share the same local database, but each context has its own migration history table.

## Development Admin

When `BT.Api` starts in Development, it seeds Identity roles and this local admin user:

- Username: `aamodhiambo@gmail.com`
- Password: `Admin@12345`
- Role: `System Administrator`

This seed is disabled outside Development.

## Run From Visual Studio

1. Open the solution.
2. Configure multiple startup projects: `BT.Api` and `BT.UI.Blazor`.
3. Start both projects with the `https` profiles.
4. Browse API documentation at `https://localhost:7129/scalar/v1`.
5. Browse the Blazor UI at `https://localhost:7049`.
6. Sign in through `/iam/sign-in` with the development admin account.
7. Use `/iam/security` to view the current user and configure authenticator-app two-factor authentication.

The Blazor app calls the API through `BackendApi:BaseUrl`. In Development this is configured as:

```json
{
  "BackendApi": {
    "BaseUrl": "https://localhost:7129/"
  }
}
```

The UI must not reference backend Application, Domain, MediatR handlers, EF Core contexts, or repositories directly. Feature UI flows should call API endpoints through typed HTTP client services and shared DTO contracts.

See `docs/architecture/ui-to-backend-flow.md` for the rule of thumb and the tradeoff behind this decision.

## Current Known Local Gaps

- Quartz tables are not created yet, so background jobs stay disabled in Development.
- Messaging is disabled in Development until the MassTransit licensing decision is made.
- The MediatR package currently emits a development/test license warning. This is not blocking local development, but production licensing or replacement must be decided later.
