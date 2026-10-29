namespace CrewTasks.Notifications.Email;

public class ChannelEmailQueue : IEmailQueue
{
    private const int Capacity = 100;

    private readonly Channel<EmailMessage> _channel = Channel.CreateBounded<EmailMessage>(Capacity);

    public ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    public IAsyncEnumerable<EmailMessage> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}

