namespace SharedTodo.Users.Contracts.Events;

public record EmailSignInStartedEvent(string Email, string Code) : INotification;
