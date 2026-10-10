using SharedTodo.Notifications.Email;

namespace SharedTodo.Notifications;

public static class NotificationsModuleExtensions
{
    public static IHostApplicationBuilder AddNotificationsModuleServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

        builder.Services.AddSingleton<ChannelEmailQueue>();
        builder.Services.AddSingleton<IEmailQueue>(services => services.GetRequiredService<ChannelEmailQueue>());

        builder.Services.AddHostedService<EmailSendingWorker>();

        return builder;
    }
}
