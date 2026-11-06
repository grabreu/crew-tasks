using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record GetTodoItemsQuery(Guid UserId, Guid TodoListId) : IQuery<Result<IReadOnlyList<TodoItemDto>>>;

public class GetTodoItemsValidator : AbstractValidator<GetTodoItemsQuery>
{
    public GetTodoItemsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();
    }
}

public class GetTodoItemsHandler(TodosDbContext dbContext) : IQueryHandler<GetTodoItemsQuery, Result<IReadOnlyList<TodoItemDto>>>
{
    public async ValueTask<Result<IReadOnlyList<TodoItemDto>>> Handle(GetTodoItemsQuery query, CancellationToken cancellationToken)
    {
        var isMember = await dbContext.TodoListMemberships
            .AnyAsync(x => x.UserId == query.UserId && x.TodoListId == query.TodoListId, cancellationToken);

        if (!isMember)
        {
            return Result.NotFound("Todo list not found.");
        }

        IReadOnlyList<TodoItemDto> todoItems = await dbContext.TodoItems
            .AsNoTracking()
            .Where(x => x.TodoListId == query.TodoListId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new TodoItemDto(x.Id, x.Title, x.IsCompleted, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(todoItems);
    }
}

public static class GetTodoItemsEndpoint
{
    public static RouteHandlerBuilder MapGetTodoItemsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/todo-lists/{todoListId:guid}/items", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("GetTodoItems")
            .WithSummary("Get Todo Items")
            .WithDescription("Retrieves the items of a list the current user belongs to, oldest first.")
            .Produces<IReadOnlyList<TodoItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetTodoItemsQuery(userId, todoListId), cancellationToken).ToOk();
    }
}
