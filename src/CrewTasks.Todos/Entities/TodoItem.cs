namespace CrewTasks.Todos.Entities;

public class TodoItem
{
    private TodoItem()
    {
    }

    public TodoItem(Guid todoListId, string title)
    {
        Id = Guid.CreateVersion7();
        TodoListId = todoListId;
        Title = title;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TodoListId { get; private set; }
    public string Title { get; private set; } = null!;
    public bool IsCompleted { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;
    public TodoList TodoList { get; private set; } = null!;

    public void ChangeTitle(string title)
    {
        Title = title;
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    public void Reopen()
    {
        IsCompleted = false;
    }
}
