@description('Location for all resources.')
param location string

@description('Tags applied to every resource.')
param tags object

resource sqlServer 'Microsoft.Sql/servers@2025-08-01-preview' existing = {
  name: 'sql-grabreu-prod-brs'
}

resource db 'Microsoft.Sql/servers/databases@2025-08-01-preview' = {
  parent: sqlServer
  name: 'sqldb-crewtasks'
  location: location
  tags: tags
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
  }
  properties: {
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
    autoPauseDelay: 60
    minCapacity: json('0.5')
  }
}

output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = db.name
