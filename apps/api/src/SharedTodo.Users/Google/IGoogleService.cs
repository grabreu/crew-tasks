namespace SharedTodo.Users.Google;

public interface IGoogleService
{
    Task<GoogleIdentity?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken);
}

public record GoogleIdentity(string Subject, string Email);
