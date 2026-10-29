namespace CrewTasks.Notifications.Email;

public class SmtpEmailService(IOptions<EmailOptions> options) : IEmailService
{
    private readonly EmailOptions _email = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_email.FromName, _email.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new BodyBuilder { HtmlBody = message.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(_email.Host, _email.Port, SecureSocketOptions.Auto, cancellationToken);

        if (!string.IsNullOrEmpty(_email.Username))
        {
            await client.AuthenticateAsync(_email.Username, _email.Password, cancellationToken);
        }

        await client.SendAsync(mimeMessage, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
