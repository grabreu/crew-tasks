using CrewTasks.Todos.Entities;

namespace CrewTasks.Todos.Data;

public class TodosDbContext(DbContextOptions<TodosDbContext> options) : DbContext(options)
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoListMembership> TodoListMemberships => Set<TodoListMembership>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Todos");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
