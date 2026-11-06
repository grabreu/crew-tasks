using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record ReopenTodoItemCommand(Guid UserId, Guid TodoListId, Guid TodoItemId) : ICommand<Result>;

public class ReopenTodoItemValidator : AbstractValidator<ReopenTodoItemCommand>
{
    public ReopenTodoItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();

        RuleFor(x => x.TodoItemId)
            .NotEmpty();
    }
}

public class ReopenTodoItemHandler(TodosDbContext dbContext) : ICommandHandler<ReopenTodoItemCommand, Result>
{
    public async ValueTask<Result> Handle(ReopenTodoItemCommand command, CancellationToken cancellationToken)
    {
        var todoItem = await dbContext.TodoItemsFor(command.UserId, command.TodoListId)
            .SingleOrDefaultAsync(x => x.Id == command.TodoItemId, cancellationToken);

        if (todoItem is null)
        {
            return Result.NotFound("Todo item not found.");
        }

        todoItem.Reopen();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public static class ReopenTodoItemEndpoint
{
    public static RouteHandlerBuilder MapReopenTodoItemEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/todo-lists/{todoListId:guid}/items/{todoItemId:guid}/reopen", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("ReopenTodoItem")
            .WithSummary("Reopen Todo Item")
            .WithDescription("Marks a completed item as not completed. Reopening an item that is not completed has no effect.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, Guid todoItemId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new ReopenTodoItemCommand(userId, todoListId, todoItemId), cancellationToken).ToNoContent();
    }
}
