using CrewTasks.Todos.Data;
using Microsoft.AspNetCore.Mvc;

namespace CrewTasks.Todos.Features.TodoItems;

public record ChangeTodoItemTitleCommand(Guid UserId, Guid TodoItemId, string Title, string RowVersion) : ICommand<Result<TodoItemDto>>;

public class ChangeTodoItemTitleValidator : AbstractValidator<ChangeTodoItemTitleCommand>
{
    public ChangeTodoItemTitleValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoItemId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .Must(x => Convert.TryFromBase64String(x, new byte[x.Length], out _))
            .WithMessage("'Row Version' must be the base64 value of the item, sent in the If-Match header.");
    }
}

public class ChangeTodoItemTitleHandler(TodosDbContext dbContext) : ICommandHandler<ChangeTodoItemTitleCommand, Result<TodoItemDto>>
{
    public async ValueTask<Result<TodoItemDto>> Handle(ChangeTodoItemTitleCommand command, CancellationToken cancellationToken)
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

        todoItem.ChangeTitle(command.Title);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoItemDto(todoItem.Id, todoItem.TodoListId, todoItem.Title, todoItem.IsCompleted, todoItem.CreatedAt, todoItem.RowVersion);
    }
}

public static class ChangeTodoItemTitleEndpoint
{
    public static RouteHandlerBuilder MapChangeTodoItemTitleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPut("/todo-items/{todoItemId:guid}/title", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("ChangeTodoItemTitle")
            .WithSummary("Change Todo Item Title")
            .WithDescription("Changes the title of an item of a list the current user belongs to. The item's rowVersion must be sent in the If-Match header; a stale value responds 409.")
            .Produces<TodoItemDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoItemId, ChangeTodoItemTitleRequest request, [FromHeader(Name = "If-Match")] string? ifMatch, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new ChangeTodoItemTitleCommand(userId, todoItemId, request.Title, ifMatch?.Trim('"') ?? string.Empty), cancellationToken).ToOk();
    }

    public record ChangeTodoItemTitleRequest(string Title);
}
