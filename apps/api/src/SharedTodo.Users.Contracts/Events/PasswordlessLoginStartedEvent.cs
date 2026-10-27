namespace SharedTodo.Users.Contracts.Events;

public record PasswordlessLoginStartedEvent(string Email, string Code) : INotification;
