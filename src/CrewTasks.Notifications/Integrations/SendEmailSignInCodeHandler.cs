using CrewTasks.Notifications.Email;
using CrewTasks.Users.Contracts.Events;

namespace CrewTasks.Notifications.Integrations;

public class SendEmailSignInCodeHandler(IEmailQueue emailQueue) : INotificationHandler<EmailSignInStartedEvent>
{
    public async ValueTask Handle(EmailSignInStartedEvent notification, CancellationToken cancellationToken)
    {
        var message = new EmailMessage(
            notification.Email,
            "Your Crew Tasks sign-in code",
            $"<p>Your sign-in code is <strong>{notification.Code}</strong>.</p><p>It expires in a few minutes. If you did not request it, ignore this email.</p>");

        await emailQueue.EnqueueAsync(message, cancellationToken);
    }
}
