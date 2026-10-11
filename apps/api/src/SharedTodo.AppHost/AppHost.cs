var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var usersDb = sql.AddDatabase("usersdb");

var mailpit = builder.AddMailPit("mailpit");

var jwtSecretKey = builder.AddParameter("jwt-secret-key", new GenerateParameterDefault { MinLength = 44, Special = false }, secret: true, persist: true);

builder.AddProject<Projects.SharedTodo_Api>("api")
    .WithReference(usersDb)
    .WithEnvironment("Jwt__SecretKey", jwtSecretKey)
    .WithEnvironment("Email__Host", mailpit.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("Email__Port", mailpit.Resource.PrimaryEndpoint.Property(EndpointProperty.Port))
    .WaitFor(usersDb)
    .WaitFor(mailpit);

var app = builder.Build();

await app.RunAsync();
