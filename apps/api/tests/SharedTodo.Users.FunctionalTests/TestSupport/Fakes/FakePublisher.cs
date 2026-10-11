namespace SharedTodo.Users.FunctionalTests.TestSupport.Fakes;

public class FakePublisher : IPublisher
{
    private readonly ConcurrentQueue<object> _notifications = new();

    public IReadOnlyList<object> Notifications => [.. _notifications];

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        _notifications.Enqueue(notification);
        return ValueTask.CompletedTask;
    }

    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        _notifications.Enqueue(notification);
        return ValueTask.CompletedTask;
    }

    public void Clear()
    {
        _notifications.Clear();
    }
}
