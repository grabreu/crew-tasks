param location string = 'brazilsouth'

@description('Image reference, tag or digest. Required on every deploy so a redeploy does not revert the image published by CD.')
param image string

@secure()
param jwtSecretKey string

param emailHost string

var tags = {
  project: 'crewtasks'
  env: 'prod'
  owner: 'grabreu'
}

module db 'modules/sql-database.bicep' = {
  name: 'db-deploy'
  params: {
    location: location
    tags: tags
  }
}

module api 'modules/container-app.bicep' = {
  name: 'api-deploy'
  params: {
    location: location
    tags: tags
    image: image
    sqlServerFqdn: db.outputs.sqlServerFqdn
    sqlDatabaseName: db.outputs.sqlDatabaseName
    jwtSecretKey: jwtSecretKey
    emailHost: emailHost
  }
}
