@description('Name of the App Service Plan')
param appServicePlanName string

@description('Name of the API Web App')
param apiAppName string

@description('Name of the UI Web App')
param uiAppName string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('The database provider to configure for the API.')
param databaseProvider string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true // Required for Linux
  }
}

resource apiApp 'Microsoft.Web/sites@2022-09-01' = {
  name: apiAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0' // CI will push the bits, use 8.0/9.0 as the base
      alwaysOn: true
      appSettings: [
        {
          name: 'DatabaseSettings__Provider'
          value: databaseProvider
        }
      ]
    }
  }
}

resource uiApp 'Microsoft.Web/sites@2022-09-01' = {
  name: uiAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
    }
  }
}

output apiDefaultHostName string = apiApp.properties.defaultHostName
output uiDefaultHostName string = uiApp.properties.defaultHostName
