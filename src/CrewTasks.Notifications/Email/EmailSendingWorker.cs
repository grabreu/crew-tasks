namespace CrewTasks.Notifications.Email;

public class EmailSendingWorker(ChannelEmailQueue queue, IEmailService emailService, ILogger<EmailSendingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await emailService.SendAsync(message, stoppingToken);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Email sent: {Subject}", message.Subject);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Failed to send email: {Subject}", message.Subject);
            }
        }
    }
}

