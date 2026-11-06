using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoLists;

public record DeleteTodoListCommand(Guid UserId, Guid TodoListId) : ICommand<Result>;

public class DeleteTodoListValidator : AbstractValidator<DeleteTodoListCommand>
{
    public DeleteTodoListValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();
    }
}

public class DeleteTodoListHandler(TodosDbContext dbContext) : ICommandHandler<DeleteTodoListCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteTodoListCommand command, CancellationToken cancellationToken)
    {
        var membership = await dbContext.TodoListMemberships
            .Include(x => x.TodoList)
            .SingleOrDefaultAsync(x => x.UserId == command.UserId && x.TodoListId == command.TodoListId, cancellationToken);

        if (membership is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        if (!membership.IsOwner)
        {
            return Result.Forbidden("Only the list owner can delete it.");
        }

        dbContext.TodoLists.Remove(membership.TodoList);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public static class DeleteTodoListEndpoint
{
    public static RouteHandlerBuilder MapDeleteTodoListEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapDelete("/todo-lists/{todoListId:guid}", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoLists")
            .WithName("DeleteTodoList")
            .WithSummary("Delete Todo List")
            .WithDescription("Deletes a list and its memberships. Only the owner can delete it.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new DeleteTodoListCommand(userId, todoListId), cancellationToken).ToNoContent();
    }
}
