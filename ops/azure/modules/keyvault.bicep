@description('The name of the Key Vault')
param keyVaultName string

@description('The location of the Key Vault')
param location string

@description('The Principal ID of the API application (to grant access)')
param apiPrincipalId string = ''

@description('The Principal ID of the UI application (to grant access)')
param uiPrincipalId string = ''

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForDeployment: true
    enabledForTemplateDeployment: true
  }
}

// Built-in Role ID for "Key Vault Secrets User"
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

// Grant API App access to Key Vault Secrets
resource apiKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(apiPrincipalId)) {
  name: guid(keyVault.id, apiPrincipalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: apiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant UI App access to Key Vault Secrets
resource uiKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(uiPrincipalId)) {
  name: guid(keyVault.id, uiPrincipalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: uiPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output keyVaultUri string = keyVault.properties.vaultUri
