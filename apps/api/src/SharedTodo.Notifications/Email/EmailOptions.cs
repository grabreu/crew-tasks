namespace SharedTodo.Notifications.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    [Required] public string Host { get; init; } = string.Empty;
    [Range(1, 65535)] public int Port { get; init; }
    [Required, EmailAddress] public string FromAddress { get; init; } = string.Empty;
    [Required] public string FromName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
