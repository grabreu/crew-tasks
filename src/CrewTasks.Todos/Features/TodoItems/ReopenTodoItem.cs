using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record ReopenTodoItemCommand(Guid UserId, Guid TodoItemId, string RowVersion) : ICommand<Result<TodoItemDto>>;

public class ReopenTodoItemValidator : AbstractValidator<ReopenTodoItemCommand>
{
    public ReopenTodoItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoItemId)
            .NotEmpty();

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .Must(x => Convert.TryFromBase64String(x, new byte[x.Length], out _))
            .WithMessage("'Row Version' must be the base64 value of the item, sent in the If-Match header.");
    }
}

public class ReopenTodoItemHandler(TodosDbContext dbContext) : ICommandHandler<ReopenTodoItemCommand, Result<TodoItemDto>>
{
    public async ValueTask<Result<TodoItemDto>> Handle(ReopenTodoItemCommand command, CancellationToken cancellationToken)
    {
        var todoItem = await dbContext.TodoItems
            .SingleOrDefaultAsync(x => x.Id == command.TodoItemId && x.TodoList.Memberships.Any(m => m.UserId == command.UserId), cancellationToken);

        if (todoItem is null)
        {
            return Result.NotFound("Todo item not found.");
        }

        if (!todoItem.RowVersion.SequenceEqual(Convert.FromBase64String(command.RowVersion)))
        {
            return Result.Conflict("The todo item was changed by someone else. Reload it and try again.");
        }

        todoItem.Reopen();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoItemDto(todoItem.Id, todoItem.TodoListId, todoItem.Title, todoItem.IsCompleted, todoItem.CreatedAt, todoItem.RowVersion);
    }
}

public static class ReopenTodoItemEndpoint
{
    public static RouteHandlerBuilder MapReopenTodoItemEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/todo-items/{todoItemId:guid}/reopen", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("ReopenTodoItem")
            .WithSummary("Reopen Todo Item")
            .WithDescription("Marks a completed item as not completed. Reopening an item that is not completed has no effect. The item's rowVersion must be sent in the If-Match header; a stale value responds 409.")
            .Produces<TodoItemDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoItemId, [FromHeader(Name = "If-Match")] string? ifMatch, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new ReopenTodoItemCommand(userId, todoItemId, ifMatch?.Trim('"') ?? string.Empty), cancellationToken).ToOk();
    }
}
