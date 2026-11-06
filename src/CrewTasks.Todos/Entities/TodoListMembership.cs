namespace CrewTasks.Todos.Entities;

public class TodoListMembership
{
    private TodoListMembership()
    {
    }

    public TodoListMembership(Guid todoListId, Guid userId, TodoListRole role)
    {
        TodoListId = todoListId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTimeOffset.UtcNow;
    }

    public Guid TodoListId { get; private set; }
    public Guid UserId { get; private set; }
    public TodoListRole Role { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public TodoList TodoList { get; private set; } = null!;

    public bool IsOwner => Role == TodoListRole.Owner;
}
