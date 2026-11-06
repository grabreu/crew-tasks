namespace CrewTasks.Todos.Entities;

public class TodoList
{
    private readonly List<TodoListMembership> _memberships = [];
    private readonly List<TodoItem> _items = [];

    private TodoList()
    {
    }

    public TodoList(string name, Guid ownerId)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        OwnerId = ownerId;
        CreatedAt = DateTimeOffset.UtcNow;

        AddMember(ownerId, TodoListRole.Owner);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<TodoListMembership> Memberships => _memberships;
    public IReadOnlyCollection<TodoItem> Items => _items;

    public void Rename(string name)
    {
        Name = name;
    }

    public void AddMember(Guid userId, TodoListRole role = TodoListRole.Member)
    {
        _memberships.Add(new TodoListMembership(Id, userId, role));
    }

    public TodoItem AddItem(string title)
    {
        var item = new TodoItem(Id, title);
        _items.Add(item);

        return item;
    }
}
