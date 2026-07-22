@description('Name of the Container Apps Environment')
param environmentName string

@description('Name of the Log Analytics Workspace')
param logAnalyticsWorkspaceName string

@description('Name of the Azure Container Registry (must be globally unique and alphanumeric only)')
param containerRegistryName string

@description('Name of the API Container App')
param apiAppName string

@description('Name of the Blazor UI Container App')
param uiAppName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The database provider to configure for the API.')
param databaseProvider string

@description('The URI of the Key Vault (optional)')
param keyVaultUri string = ''

@description('The name of the Storage Account (optional)')
param storageAccountName string = ''

// Container Registry
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Container Apps Environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

// Stub for API App (to be overwritten by CI)
resource apiApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: apiAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'api'
          image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          env: [
            {
              name: 'DatabaseSettings__Provider'
              value: databaseProvider
            }
            {
              name: 'KeyVault__Uri'
              value: keyVaultUri
            }
            {
              name: 'ProfileImageStorage__AzureBlob__ContainerUri'
              value: empty(storageAccountName) ? '' : 'https://${storageAccountName}.blob.${environment().suffixes.storage}/profile-images'
            }
            {
              name: 'ProfileImageStorage__Provider'
              value: 'AzureBlob'
            }
            {
              name: 'DataProtection__BlobKeyUri'
              value: empty(storageAccountName) ? '' : 'https://${storageAccountName}.blob.${environment().suffixes.storage}/dataprotection-keys/keyring.xml'
            }
            {
              name: 'AllowedOrigins__0'
              value: 'https://${uiAppName}.${containerAppEnv.properties.defaultDomain}'
            }
            {
              name: 'Messaging__Transport'
              value: 'AzureServiceBus'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
      }
    }
  }
}

// Stub for UI App (to be overwritten by CI)
resource uiApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: uiAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'ui'
          image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          env: [
            {
              name: 'BackendApi__BaseUrl'
              value: 'https://${apiAppName}.${containerAppEnv.properties.defaultDomain}/'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
      }
    }
  }
}

output acrLoginServer string = acr.properties.loginServer
output apiFqdn string = apiApp.properties.configuration.ingress.fqdn
output uiFqdn string = uiApp.properties.configuration.ingress.fqdn
output apiPrincipalId string = apiApp.identity.principalId
output uiPrincipalId string = uiApp.identity.principalId
