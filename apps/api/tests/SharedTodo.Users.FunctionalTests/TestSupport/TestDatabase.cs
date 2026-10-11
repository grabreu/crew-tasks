using SharedTodo.Users.Persistence;

namespace SharedTodo.Users.FunctionalTests.TestSupport;

public class TestDatabase : IAsyncDisposable
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    private SqlConnection? _connection;
    private Respawner? _respawner;

    public string ConnectionString => _container.GetConnectionString();

    public Task StartAsync(CancellationToken cancellationToken) => _container.StartAsync(cancellationToken);

    public async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await context.Database.MigrateAsync(cancellationToken);

        await InitializeRespawn();
    }

    private async Task InitializeRespawn()
    {
        _connection = new SqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public Task ResetAsync()
    {
        if (_respawner is not null && _connection is not null)
        {
            return _respawner.ResetAsync(_connection);
        }

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _container.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}
