using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record DeleteTodoItemCommand(Guid UserId, Guid TodoItemId, string RowVersion) : ICommand<Result>;

public class DeleteTodoItemValidator : AbstractValidator<DeleteTodoItemCommand>
{
    public DeleteTodoItemValidator()
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

public class DeleteTodoItemHandler(TodosDbContext dbContext) : ICommandHandler<DeleteTodoItemCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteTodoItemCommand command, CancellationToken cancellationToken)
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

        dbContext.TodoItems.Remove(todoItem);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public static class DeleteTodoItemEndpoint
{
    public static RouteHandlerBuilder MapDeleteTodoItemEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapDelete("/todo-items/{todoItemId:guid}", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("DeleteTodoItem")
            .WithSummary("Delete Todo Item")
            .WithDescription("Deletes an item of a list the current user belongs to. The item's rowVersion must be sent in the If-Match header; a stale value responds 409.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoItemId, [FromHeader(Name = "If-Match")] string? ifMatch, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new DeleteTodoItemCommand(userId, todoItemId, ifMatch?.Trim('"') ?? string.Empty), cancellationToken).ToNoContent();
    }
}
