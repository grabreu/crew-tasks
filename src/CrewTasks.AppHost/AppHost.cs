var builder = DistributedApplication.CreateBuilder(args);

var googleClientId = builder.AddParameter("google-client-id");
var jwtSecretKey = builder.AddParameter(
    "jwt-secret-key",
    new GenerateParameterDefault { MinLength = 44, Special = false },
    secret: true,
    persist: true);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var usersDb = sql.AddDatabase("usersdb");

var mailpit = builder.AddMailPit("mailpit");

builder.AddProject<Projects.CrewTasks_Api>("api")
    .WithReference(usersDb)
    .WaitFor(usersDb)
    .WithEnvironment("Authentication__Google__ClientId", googleClientId)
    .WithEnvironment("Jwt__SecretKey", jwtSecretKey)
    .WithEnvironment("Email__Host", mailpit.Resource.PrimaryEndpoint.Property(EndpointProperty.Host))
    .WithEnvironment("Email__Port", mailpit.Resource.PrimaryEndpoint.Property(EndpointProperty.Port))
    .WaitFor(mailpit);

var app = builder.Build();

await app.RunAsync();
