using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record CompleteTodoItemCommand(Guid UserId, Guid TodoItemId, string RowVersion) : ICommand<Result<TodoItemDto>>;

public class CompleteTodoItemValidator : AbstractValidator<CompleteTodoItemCommand>
{
    public CompleteTodoItemValidator()
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

public class CompleteTodoItemHandler(TodosDbContext dbContext) : ICommandHandler<CompleteTodoItemCommand, Result<TodoItemDto>>
{
    public async ValueTask<Result<TodoItemDto>> Handle(CompleteTodoItemCommand command, CancellationToken cancellationToken)
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

        todoItem.Complete();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoItemDto(todoItem.Id, todoItem.TodoListId, todoItem.Title, todoItem.IsCompleted, todoItem.CreatedAt, todoItem.RowVersion);
    }
}

public static class CompleteTodoItemEndpoint
{
    public static RouteHandlerBuilder MapCompleteTodoItemEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/todo-items/{todoItemId:guid}/complete", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("CompleteTodoItem")
            .WithSummary("Complete Todo Item")
            .WithDescription("Marks an item as completed. Completing an item that is already completed has no effect. The item's rowVersion must be sent in the If-Match header; a stale value responds 409.")
            .Produces<TodoItemDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoItemId, [FromHeader(Name = "If-Match")] string? ifMatch, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new CompleteTodoItemCommand(userId, todoItemId, ifMatch?.Trim('"') ?? string.Empty), cancellationToken).ToOk();
    }
}
