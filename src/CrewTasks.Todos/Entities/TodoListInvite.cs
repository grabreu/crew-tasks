namespace CrewTasks.Todos.Entities;

public class TodoListInvite
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    private TodoListInvite()
    {
    }

    public TodoListInvite(Guid todoListId, Guid createdByUserId)
    {
        Id = Guid.CreateVersion7();
        TodoListId = todoListId;
        CreatedByUserId = createdByUserId;
        Token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        CreatedAt = DateTimeOffset.UtcNow;
        ExpiresAt = CreatedAt + Lifetime;
    }

    public Guid Id { get; private set; }
    public Guid TodoListId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public TodoList TodoList { get; private set; } = null!;

    public bool IsUsable(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke()
    {
        RevokedAt ??= DateTimeOffset.UtcNow;
    }
}
