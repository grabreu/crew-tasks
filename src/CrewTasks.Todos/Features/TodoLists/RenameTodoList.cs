using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoLists;

public record RenameTodoListCommand(Guid UserId, Guid TodoListId, string Name) : ICommand<Result>;

public class RenameTodoListValidator : AbstractValidator<RenameTodoListCommand>
{
    public RenameTodoListValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class RenameTodoListHandler(TodosDbContext dbContext) : ICommandHandler<RenameTodoListCommand, Result>
{
    public async ValueTask<Result> Handle(RenameTodoListCommand command, CancellationToken cancellationToken)
    {
        var todoList = await dbContext.TodoListMemberships
            .Where(x => x.UserId == command.UserId && x.TodoListId == command.TodoListId)
            .Select(x => x.TodoList)
            .SingleOrDefaultAsync(cancellationToken);

        if (todoList is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        todoList.Rename(command.Name);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public static class RenameTodoListEndpoint
{
    public static RouteHandlerBuilder MapRenameTodoListEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPut("/todo-lists/{todoListId:guid}", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoLists")
            .WithName("RenameTodoList")
            .WithSummary("Rename Todo List")
            .WithDescription("Renames a list the current user belongs to.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, RenameTodoListRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new RenameTodoListCommand(userId, todoListId, request.Name), cancellationToken).ToNoContent();
    }

    public record RenameTodoListRequest(string Name);
}
