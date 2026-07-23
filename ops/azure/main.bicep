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

@description('The database provider to provision and configure.')
@allowed([
  'SqlServer'
  'PostgreSql'
])
param databaseProvider string = 'SqlServer'

@description('Optional Azure Cache for Redis connection string. When empty, cache falls back to the configured non-Azure provider.')
@secure()
param azureCacheConnectionString string = ''

// Resource Names
var sqlServerName = '${resourcePrefix}-sql-${uniqueString(resourceGroup().id)}'
var acrName = '${resourcePrefix}acr${uniqueString(resourceGroup().id)}'
var logAnalyticsName = '${resourcePrefix}-law-${uniqueString(resourceGroup().id)}'
var acaEnvName = '${resourcePrefix}-env-${uniqueString(resourceGroup().id)}'
var apiAppName = '${resourcePrefix}-api'
var uiAppName = '${resourcePrefix}-ui'
var appServicePlanName = '${resourcePrefix}-asp'
var keyVaultName = take('${resourcePrefix}kv${uniqueString(resourceGroup().id)}', 24)
var keyVaultUri = 'https://${keyVaultName}${environment().suffixes.keyvaultDns}/'
var storageAccountName = take('${resourcePrefix}st${uniqueString(resourceGroup().id)}', 24)
var apiPrincipalId = deploymentTarget == 'container-apps' ? (containerApps.outputs.?apiPrincipalId ?? '') : (appService.outputs.?apiPrincipalId ?? '')

module sql 'modules/sql.bicep' = if (databaseProvider == 'SqlServer') {
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

module postgres 'modules/postgres.bicep' = if (databaseProvider == 'PostgreSql') {
  name: 'postgresDeploy'
  params: {
    serverName: sqlServerName // Using same naming convention for simplicity
    databaseName: 'bt'
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
    databaseProvider: databaseProvider
    keyVaultUri: keyVaultUri
    storageAccountName: storageAccountName
    dataProtectionKeyIdentifier: '${keyVaultUri}keys/dataprotection'
  }
}

module appService 'modules/appservice.bicep' = if (deploymentTarget == 'app-service') {
  name: 'appServiceDeploy'
  params: {
    appServicePlanName: appServicePlanName
    apiAppName: apiAppName
    uiAppName: uiAppName
    location: location
    databaseProvider: databaseProvider
    keyVaultUri: keyVaultUri
    storageAccountName: storageAccountName
    dataProtectionKeyIdentifier: '${keyVaultUri}keys/dataprotection'
  }
}

var serviceBusName = '${resourcePrefix}-sb-${uniqueString(resourceGroup().id)}'

module serviceBus 'modules/servicebus.bicep' = {
  name: 'serviceBusDeploy'
  params: {
    namespaceName: serviceBusName
    location: location
    sku: 'Standard'
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyVaultDeploy'
  params: {
    keyVaultName: keyVaultName
    location: location
    apiPrincipalId: apiPrincipalId
    databaseConnectionString: databaseProvider == 'SqlServer'
      ? 'Server=tcp:${sql.outputs.?serverFullyQualifiedDomainName ?? ''},1433;Initial Catalog=${sql.outputs.?databaseName ?? ''};Persist Security Info=False;User ID=${sqlAdministratorLogin};Password=${sqlAdministratorLoginPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
      : 'Host=${postgres.outputs.?serverFullyQualifiedDomainName ?? ''};Port=5432;Database=${postgres.outputs.?databaseName ?? ''};Username=${sqlAdministratorLogin};Password=${sqlAdministratorLoginPassword};Ssl Mode=Require;Trust Server Certificate=false;'
    serviceBusNamespaceName: serviceBus.outputs.serviceBusNamespaceName
    azureCacheConnectionString: azureCacheConnectionString
  }
}

module storageAccount 'modules/storage.bicep' = {
  name: 'storageAccountDeploy'
  params: {
    storageAccountName: storageAccountName
    location: location
    apiPrincipalId: apiPrincipalId
  }
}

output sqlServerFqdn string = databaseProvider == 'SqlServer' ? (sql.outputs.?serverFullyQualifiedDomainName ?? '') : (postgres.outputs.?serverFullyQualifiedDomainName ?? '')
output sqlDatabaseName string = databaseProvider == 'SqlServer' ? (sql.outputs.?databaseName ?? '') : (postgres.outputs.?databaseName ?? '')

output acrLoginServer string = deploymentTarget == 'container-apps' ? (containerApps.outputs.?acrLoginServer ?? '') : ''
output apiHost string = deploymentTarget == 'container-apps' ? (containerApps.outputs.?apiFqdn ?? '') : (appService.outputs.?apiDefaultHostName ?? '')
output uiHost string = deploymentTarget == 'container-apps' ? (containerApps.outputs.?uiFqdn ?? '') : (appService.outputs.?uiDefaultHostName ?? '')
