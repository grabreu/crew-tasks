using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record CompleteTodoItemCommand(Guid UserId, Guid TodoItemId) : ICommand<Result>;

public class CompleteTodoItemValidator : AbstractValidator<CompleteTodoItemCommand>
{
    public CompleteTodoItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoItemId)
            .NotEmpty();
    }
}

public class CompleteTodoItemHandler(TodosDbContext dbContext) : ICommandHandler<CompleteTodoItemCommand, Result>
{
    public async ValueTask<Result> Handle(CompleteTodoItemCommand command, CancellationToken cancellationToken)
    {
        var todoItem = await dbContext.TodoItems
            .SingleOrDefaultAsync(x => x.Id == command.TodoItemId && x.TodoList.Memberships.Any(m => m.UserId == command.UserId), cancellationToken);

        if (todoItem is null)
        {
            return Result.NotFound("Todo item not found.");
        }

        todoItem.Complete();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
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
            .WithDescription("Marks an item as completed. Completing an item that is already completed has no effect.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoItemId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new CompleteTodoItemCommand(userId, todoItemId), cancellationToken).ToNoContent();
    }
}
