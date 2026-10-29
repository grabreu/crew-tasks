namespace CrewTasks.Notifications.Email;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailMessage message, CancellationToken cancellationToken);
}
