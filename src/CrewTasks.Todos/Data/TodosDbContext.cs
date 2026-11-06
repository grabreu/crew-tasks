using CrewTasks.Todos.Entities;

namespace CrewTasks.Todos.Data;

public class TodosDbContext(DbContextOptions<TodosDbContext> options) : DbContext(options)
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoListMembership> TodoListMemberships => Set<TodoListMembership>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public IQueryable<TodoItem> TodoItemsFor(Guid userId, Guid todoListId)
    {
        return TodoItems.Where(x => x.TodoListId == todoListId
            && TodoListMemberships.Any(m => m.TodoListId == todoListId && m.UserId == userId));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Todos");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
