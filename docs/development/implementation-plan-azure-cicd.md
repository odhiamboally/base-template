# Implementation Plan - GitHub Actions to Azure CI/CD Pipeline

This document outlines the implementation plan for automating the build, test, and deployment flow of BaseTemplate's backend API and frontend Blazor UI to Azure App Service using GitHub Actions.

## User Review Required

> [!IMPORTANT]
> To execute this deployment workflow successfully in GitHub Actions, you need to save the **Publish Profile** credentials for both Azure App Services as repository secrets:
> - `AZURE_API_PUBLISH_PROFILE` (for the backend API App Service)
> - `AZURE_UI_PUBLISH_PROFILE` (for the Blazor UI App Service)
> 
> *To download the publish profile: Go to the Azure Portal -> Navigate to each App Service -> In the **Overview** page, click **Get publish profile**.*

## Proposed Changes

We will introduce a new deployment workflow in `.github/workflows/` and ensure our environment configuration checklist documents how deployment configuration maps to Azure.

---

### [CI/CD Workflow]

#### [NEW] [deploy.yml](file:///e:/Repos/BaseTemplate/.github/workflows/deploy.yml)
Create a new GitHub Actions workflow that:
- Triggers on `push` (or merge) to the `main` branch, or manually via `workflow_dispatch`.
- Restores, builds, and runs unit, integration, and architecture tests in `Release` configuration.
- Publishes the compiled artifacts for the API and UI.
- Deploys both applications to Azure App Services using `azure/webapps-deploy@v3`.

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [main]
    paths:
      - "src/**"
      - ".github/workflows/deploy.yml"
  workflow_dispatch:

permissions:
  contents: read

jobs:
  build-and-test:
    name: Build & Test
    runs-on: windows-latest
    env:
      FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Restore dependencies
        run: dotnet restore BaseTemplate.sln

      - name: Build Solution
        run: dotnet build BaseTemplate.sln --configuration Release --no-restore

      - name: Run Architecture Tests
        run: dotnet test tests/BT.Tests.Architecture/BT.Tests.Architecture.csproj --configuration Release --no-restore --no-build

      - name: Run Unit Tests
        run: dotnet test tests/BT.Tests.Unit/BT.Tests.Unit.csproj --configuration Release --no-restore --no-build

      - name: Run Integration Tests
        run: dotnet test tests/BT.Tests.Integration/BT.Tests.Integration.csproj --configuration Release --no-restore --no-build

      - name: Publish API
        run: dotnet publish src/Backend/Api/BT.Api/BT.Api.csproj --configuration Release --no-restore --no-build --output ./publish-api

      - name: Publish Blazor UI
        run: dotnet publish src/Frontend/Web/BT.UI.Blazor/BT.UI.Blazor.csproj --configuration Release --no-restore --no-build --output ./publish-ui

      - name: Upload API Artifacts
        uses: actions/upload-artifact@v4
        with:
          name: api-app
          path: ./publish-api

      - name: Upload UI Artifacts
        uses: actions/upload-artifact@v4
        with:
          name: ui-app
          path: ./publish-ui

  deploy-api:
    name: Deploy API to Azure
    needs: build-and-test
    runs-on: ubuntu-latest
    steps:
      - name: Download API Artifacts
        uses: actions/download-artifact@v4
        with:
          name: api-app
          path: ./api-app

      - name: Deploy to Azure Web App (API)
        uses: azure/webapps-deploy@v3
        with:
          app-name: 'basetemplate-api' # Placeholder: User's API App Service name
          publish-profile: ${{ secrets.AZURE_API_PUBLISH_PROFILE }}
          package: ./api-app

  deploy-ui:
    name: Deploy UI to Azure
    needs: build-and-test
    runs-on: ubuntu-latest
    steps:
      - name: Download UI Artifacts
        uses: actions/download-artifact@v4
        with:
          name: ui-app
          path: ./ui-app

      - name: Deploy to Azure Web App (UI)
        uses: azure/webapps-deploy@v3
        with:
          app-name: 'basetemplate-ui' # Placeholder: User's UI App Service name
          publish-profile: ${{ secrets.AZURE_UI_PUBLISH_PROFILE }}
          package: ./ui-app
```

---

### [Documentation Updates]

#### [MODIFY] [environment-configuration-checklist.md](file:///e:/Repos/BaseTemplate/docs/development/environment-configuration-checklist.md)
Update the configuration documentation to detail:
- Production environment configurations.
- Mapping connection strings and secrets to Azure App Service settings or Key Vault.

---

## Verification Plan

### Automated Verification
- Run a syntax check on the YAML workflow syntax.
- Verify that `dotnet publish` executes successfully for both projects locally under `Release` configuration:
  - `dotnet publish src/Backend/Api/BT.Api/BT.Api.csproj --configuration Release`
  - `dotnet publish src/Frontend/Web/BT.UI.Blazor/BT.UI.Blazor.csproj --configuration Release`

### Manual Verification
- After push/merge to `main`, check the GitHub Actions tab to confirm the workflow starts, restores, builds, runs tests, publishes artifacts, and initiates deployment to the designated App Services.
- Configure secrets `AZURE_API_PUBLISH_PROFILE` and `AZURE_UI_PUBLISH_PROFILE` in GitHub settings.
