using SharedTodo.Notifications.Email;
using SharedTodo.Users.Contracts.Events;

namespace SharedTodo.Notifications.Integrations;

public class SendPasswordlessLoginCodeHandler(IEmailQueue emailQueue) : INotificationHandler<PasswordlessLoginStartedEvent>
{
    public async ValueTask Handle(PasswordlessLoginStartedEvent notification, CancellationToken cancellationToken)
    {
        var message = new EmailMessage(
            notification.Email,
            "Your SharedTodo login code",
            $"<p>Your login code is <strong>{notification.Code}</strong>.</p><p>It expires in a few minutes. If you did not request it, ignore this email.</p>");

        await emailQueue.EnqueueAsync(message, cancellationToken);
    }
}
