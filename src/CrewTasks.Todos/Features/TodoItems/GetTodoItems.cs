using CrewTasks.Todos.Data;
using Microsoft.AspNetCore.Mvc;

namespace CrewTasks.Todos.Features.TodoItems;

public record GetTodoItemsQuery(Guid UserId, Guid? TodoListId, bool? IsCompleted) : IQuery<Result<IReadOnlyList<TodoItemDto>>>;

public class GetTodoItemsValidator : AbstractValidator<GetTodoItemsQuery>
{
    public GetTodoItemsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEqual(Guid.Empty);
    }
}

public class GetTodoItemsHandler(TodosDbContext dbContext) : IQueryHandler<GetTodoItemsQuery, Result<IReadOnlyList<TodoItemDto>>>
{
    public async ValueTask<Result<IReadOnlyList<TodoItemDto>>> Handle(GetTodoItemsQuery query, CancellationToken cancellationToken)
    {
        var todoItems = dbContext.TodoItems
            .AsNoTracking()
            .Where(x => x.TodoList.Memberships.Any(m => m.UserId == query.UserId));

        if (query.TodoListId is not null)
        {
            todoItems = todoItems.Where(x => x.TodoListId == query.TodoListId);
        }

        if (query.IsCompleted is not null)
        {
            todoItems = todoItems.Where(x => x.IsCompleted == query.IsCompleted);
        }

        IReadOnlyList<TodoItemDto> result = await todoItems
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new TodoItemDto(x.Id, x.TodoListId, x.Title, x.IsCompleted, x.CreatedAt, x.RowVersion))
            .ToListAsync(cancellationToken);

        return Result.Success(result);
    }
}

public static class GetTodoItemsEndpoint
{
    public static RouteHandlerBuilder MapGetTodoItemsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/todo-items", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("GetTodoItems")
            .WithSummary("Get Todo Items")
            .WithDescription("Retrieves the items of the lists the current user belongs to, oldest first, optionally filtered by list and completion.")
            .Produces<IReadOnlyList<TodoItemDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static ValueTask<IResult> HandleAsync([AsParameters] GetTodoItemsRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetTodoItemsQuery(userId, request.TodoListId, request.IsCompleted), cancellationToken).ToOk();
    }

    public record GetTodoItemsRequest([FromQuery] Guid? TodoListId, [FromQuery] bool? IsCompleted);
}
