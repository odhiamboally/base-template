@description('The primary location for all resources.')
param location string = resourceGroup().location

@description('The unique prefix for naming resources.')
param resourcePrefix string = 'btapp'

@description('The administrator login username for the SQL server.')
param sqlAdministratorLogin string

@description('The administrator login password for the SQL server.')
@secure()
param sqlAdministratorLoginPassword string

@description('The deployment target for the web layer.')
@allowed([
  'app-service'
  'container-apps'
])
param deploymentTarget string = 'container-apps'

// Resource Names
var sqlServerName = '${resourcePrefix}-sql-${uniqueString(resourceGroup().id)}'
var acrName = '${resourcePrefix}acr${uniqueString(resourceGroup().id)}'
var logAnalyticsName = '${resourcePrefix}-law-${uniqueString(resourceGroup().id)}'
var acaEnvName = '${resourcePrefix}-env-${uniqueString(resourceGroup().id)}'
var apiAppName = '${resourcePrefix}-api'
var uiAppName = '${resourcePrefix}-ui'
var appServicePlanName = '${resourcePrefix}-asp'

module sql 'modules/sql.bicep' = {
  name: 'sqlDeploy'
  params: {
    serverName: sqlServerName
    databaseName: 'BT'
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorLoginPassword
    location: location
    allowAzureIps: true
  }
}

module containerApps 'modules/containerapps.bicep' = if (deploymentTarget == 'container-apps') {
  name: 'acaDeploy'
  params: {
    environmentName: acaEnvName
    logAnalyticsWorkspaceName: logAnalyticsName
    containerRegistryName: acrName
    apiAppName: apiAppName
    uiAppName: uiAppName
    location: location
  }
}

module appService 'modules/appservice.bicep' = if (deploymentTarget == 'app-service') {
  name: 'appServiceDeploy'
  params: {
    appServicePlanName: appServicePlanName
    apiAppName: apiAppName
    uiAppName: uiAppName
    location: location
  }
}

output sqlServerFqdn string = sql.outputs.serverFullyQualifiedDomainName
output sqlDatabaseName string = sql.outputs.databaseName

output acrLoginServer string = deploymentTarget == 'container-apps' ? containerApps.outputs.acrLoginServer : ''
output apiHost string = deploymentTarget == 'container-apps' ? containerApps.outputs.apiFqdn : appService.outputs.apiDefaultHostName
output uiHost string = deploymentTarget == 'container-apps' ? containerApps.outputs.uiFqdn : appService.outputs.uiDefaultHostName
