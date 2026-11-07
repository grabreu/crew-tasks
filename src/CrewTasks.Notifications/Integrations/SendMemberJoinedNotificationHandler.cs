using CrewTasks.Notifications.Email;
using CrewTasks.Todos.Contracts.Events;
using CrewTasks.Users.Contracts;

namespace CrewTasks.Notifications.Integrations;

public class SendMemberJoinedNotificationHandler(IUserDirectory userDirectory, IEmailQueue emailQueue, ILogger<SendMemberJoinedNotificationHandler> logger) : INotificationHandler<TodoListMemberJoinedEvent>
{
    public async ValueTask Handle(TodoListMemberJoinedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var users = await userDirectory.GetUsersAsync([notification.OwnerId, notification.MemberId], cancellationToken);

            var ownerEmail = users.GetValueOrDefault(notification.OwnerId)?.Email;

            if (string.IsNullOrEmpty(ownerEmail))
            {
                logger.LogWarning("Skipping member joined email for list {TodoListId}: owner {OwnerId} has no email.", notification.TodoListId, notification.OwnerId);
                return;
            }

            var memberName = users.GetValueOrDefault(notification.MemberId)?.DisplayName ?? "Someone";

            var message = new EmailMessage(
                ownerEmail,
                $"{memberName} joined {notification.TodoListName}",
                $"<p><strong>{WebUtility.HtmlEncode(memberName)}</strong> joined your list <strong>{WebUtility.HtmlEncode(notification.TodoListName)}</strong> using an invite link.</p>");

            await emailQueue.EnqueueAsync(message, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Could not send the member joined email for list {TodoListId}.", notification.TodoListId);
        }
    }
}
