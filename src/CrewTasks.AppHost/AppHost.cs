var builder = DistributedApplication.CreateBuilder(args);

var jwtSecretKey = builder.AddParameter(
    "jwt-secret-key",
    new GenerateParameterDefault { MinLength = 44, Special = false },
    secret: true,
    persist: true);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var sqldb = sql.AddDatabase("sqldb");

var mailpit = builder.AddMailPit("mailpit");

builder.AddProject<Projects.CrewTasks_Api>("api")
    .WithReference(sqldb, "UsersDb")
    .WithReference(sqldb, "TodosDb")
    .WithEnvironment("Jwt__SecretKey", jwtSecretKey)
    .WithEnvironment("Email__Host", mailpit.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("Email__Port", mailpit.Resource.PrimaryEndpoint.Property(EndpointProperty.Port))
    .WaitFor(sqldb)
    .WaitFor(mailpit);

var app = builder.Build();

await app.RunAsync();
