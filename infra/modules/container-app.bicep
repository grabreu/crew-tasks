@description('Location for all resources.')
param location string

@description('Tags applied to every resource.')
param tags object

@description('Image reference, tag or digest.')
param image string

param sqlServerFqdn string
param sqlDatabaseName string

@secure()
param jwtSecretKey string

param emailHost string

resource cae 'Microsoft.App/managedEnvironments@2026-01-01' existing = {
  name: 'cae-grabreu-prod-brs'
}

resource acr 'Microsoft.ContainerRegistry/registries@2025-04-01' existing = {
  name: 'crgrabreuprod'
}

resource api 'Microsoft.App/containerApps@2026-01-01' = {
  name: 'ca-crewtasks-api-prod-brs'
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    environmentId: cae.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: 'system'
        }
      ]
      secrets: [
        {
          name: 'jwt-secret-key'
          value: jwtSecretKey
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: image
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ConnectionStrings__UsersDb'
              value: 'Server=tcp:${sqlServerFqdn},1433;Database=${sqlDatabaseName};Authentication=Active Directory Managed Identity;Encrypt=True'
            }
            {
              name: 'Jwt__SecretKey'
              secretRef: 'jwt-secret-key'
            }
            {
              name: 'Email__Host'
              value: emailHost
            }
          ]
        }
      ]
    }
  }
}
