using CrewTasks.Todos.Data;
using CrewTasks.Todos.Features;

namespace CrewTasks.Todos;

public static class TodosModuleExtensions
{
    public const string Schema = "Todos";
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public static IHostApplicationBuilder AddTodosModuleServices(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<TodosDbContext>("TodosDb", configureDbContextOptions: options => options.UseSqlServer(sql => sql.MigrationsHistoryTable(MigrationsHistoryTable, Schema)));

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return builder;
    }

    public static IEndpointRouteBuilder MapTodosModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCreateTodoListEndpoint();
        endpoints.MapGetTodoListsEndpoint();
        endpoints.MapGetTodoListEndpoint();
        endpoints.MapRenameTodoListEndpoint();
        endpoints.MapDeleteTodoListEndpoint();

        return endpoints;
    }

    public static async Task EnsureTodosModuleDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TodosDbContext>();
        await context.Database.MigrateAsync();
    }
}
