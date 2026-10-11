using SharedTodo.Users.FunctionalTests.TestSupport.Fakes;
using SharedTodo.Users.FunctionalTests.TestSupport.Seeders;

namespace SharedTodo.Users.FunctionalTests.TestSupport;

[Collection(nameof(FunctionalTestCollection))]
public abstract class FunctionalTestBase(FunctionalTestFixture fixture) : IAsyncLifetime
{
    protected FakePublisher Publisher => fixture.Publisher;
    protected UserSeeder Users => new(fixture.Services);

    public ValueTask InitializeAsync()
    {
        return fixture.ResetStateAsync();
    }

    protected async ValueTask<TResponse> SendAsync<TResponse>(ICommand<TResponse> command)
    {
        using var scope = fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(command, TestContext.Current.CancellationToken);
    }

    protected async ValueTask<TResponse> SendAsync<TResponse>(IQuery<TResponse> query)
    {
        using var scope = fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(query, TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
