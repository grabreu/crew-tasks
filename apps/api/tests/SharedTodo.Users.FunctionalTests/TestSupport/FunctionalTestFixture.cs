using SharedTodo.Users.FunctionalTests.TestSupport.Fakes;

namespace SharedTodo.Users.FunctionalTests.TestSupport;

public class FunctionalTestFixture : IAsyncLifetime
{
    private readonly TestDatabase _database = new();

    private IHost? _host;

    public FakePublisher Publisher { get; } = new();
    public IServiceProvider Services => _host!.Services;

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync(TestContext.Current.CancellationToken);

        _host = BuildHost(_database.ConnectionString);

        await _database.InitializeAsync(_host.Services, TestContext.Current.CancellationToken);
    }

    private IHost BuildHost(string connectionString)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:UsersDb"] = connectionString
        });

        builder.Services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        builder.Services.RemoveAll<IPublisher>();
        builder.Services.AddSingleton<IPublisher>(Publisher);

        builder.AddUsersModuleServices();

        return builder.Build();
    }

    public async ValueTask ResetStateAsync()
    {
        await _database.ResetAsync();

        Publisher.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        _host?.Dispose();

        await _database.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}
