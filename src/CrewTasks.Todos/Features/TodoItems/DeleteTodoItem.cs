using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record DeleteTodoItemCommand(Guid UserId, Guid TodoItemId) : ICommand<Result>;

public class DeleteTodoItemValidator : AbstractValidator<DeleteTodoItemCommand>
{
    public DeleteTodoItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoItemId)
            .NotEmpty();
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
            .WithDescription("Deletes an item of a list the current user belongs to.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoItemId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new DeleteTodoItemCommand(userId, todoItemId), cancellationToken).ToNoContent();
    }
}
