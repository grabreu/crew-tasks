using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record ChangeTodoItemTitleCommand(Guid UserId, Guid TodoListId, Guid TodoItemId, string Title) : ICommand<Result>;

public class ChangeTodoItemTitleValidator : AbstractValidator<ChangeTodoItemTitleCommand>
{
    public ChangeTodoItemTitleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();

        RuleFor(x => x.TodoItemId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public class ChangeTodoItemTitleHandler(TodosDbContext dbContext) : ICommandHandler<ChangeTodoItemTitleCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeTodoItemTitleCommand command, CancellationToken cancellationToken)
    {
        var todoItem = await dbContext.TodoItemsFor(command.UserId, command.TodoListId)
            .SingleOrDefaultAsync(x => x.Id == command.TodoItemId, cancellationToken);

        if (todoItem is null)
        {
            return Result.NotFound("Todo item not found.");
        }

        todoItem.ChangeTitle(command.Title);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public static class ChangeTodoItemTitleEndpoint
{
    public static RouteHandlerBuilder MapChangeTodoItemTitleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPut("/todo-lists/{todoListId:guid}/items/{todoItemId:guid}/title", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("ChangeTodoItemTitle")
            .WithSummary("Change Todo Item Title")
            .WithDescription("Changes the title of an item of a list the current user belongs to.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, Guid todoItemId, ChangeTodoItemTitleRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new ChangeTodoItemTitleCommand(userId, todoListId, todoItemId, request.Title), cancellationToken).ToNoContent();
    }

    public record ChangeTodoItemTitleRequest(string Title);
}
