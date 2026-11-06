namespace CrewTasks.Todos.Data;

public class TodosDbContextFactory : IDesignTimeDbContextFactory<TodosDbContext>
{
    public TodosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TodosDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TodosDb;Trusted_Connection=True;MultipleActiveResultSets=true", sql => sql.MigrationsHistoryTable(TodosModuleExtensions.MigrationsHistoryTable, TodosModuleExtensions.Schema));

        return new TodosDbContext(optionsBuilder.Options);
    }
}
