using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record GetTodoItemQuery(Guid UserId, Guid TodoListId, Guid TodoItemId) : IQuery<Result<TodoItemDto>>;

public class GetTodoItemValidator : AbstractValidator<GetTodoItemQuery>
{
    public GetTodoItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();

        RuleFor(x => x.TodoItemId)
            .NotEmpty();
    }
}

public class GetTodoItemHandler(TodosDbContext dbContext) : IQueryHandler<GetTodoItemQuery, Result<TodoItemDto>>
{
    public async ValueTask<Result<TodoItemDto>> Handle(GetTodoItemQuery query, CancellationToken cancellationToken)
    {
        var todoItem = await dbContext.TodoItemsFor(query.UserId, query.TodoListId)
            .AsNoTracking()
            .Where(x => x.Id == query.TodoItemId)
            .Select(x => new TodoItemDto(x.Id, x.Title, x.IsCompleted, x.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (todoItem is null)
        {
            return Result.NotFound("Todo item not found.");
        }

        return todoItem;
    }
}

public static class GetTodoItemEndpoint
{
    public static RouteHandlerBuilder MapGetTodoItemEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/todo-lists/{todoListId:guid}/items/{todoItemId:guid}", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("GetTodoItem")
            .WithSummary("Get Todo Item")
            .WithDescription("Retrieves an item of a list the current user belongs to.")
            .Produces<TodoItemDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, Guid todoItemId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetTodoItemQuery(userId, todoListId, todoItemId), cancellationToken).ToOk();
    }
}
