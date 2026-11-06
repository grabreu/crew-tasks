using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features;

public record GetTodoListQuery(Guid UserId, Guid TodoListId) : IQuery<Result<TodoListDto>>;

public class GetTodoListValidator : AbstractValidator<GetTodoListQuery>
{
    public GetTodoListValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();
    }
}

public class GetTodoListHandler(TodosDbContext dbContext) : IQueryHandler<GetTodoListQuery, Result<TodoListDto>>
{
    public async ValueTask<Result<TodoListDto>> Handle(GetTodoListQuery query, CancellationToken cancellationToken)
    {
        var todoList = await dbContext.TodoListMemberships
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId && x.TodoListId == query.TodoListId)
            .Select(x => new TodoListDto(x.TodoList.Id, x.TodoList.Name, x.TodoList.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (todoList is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        return todoList;
    }
}

public static class GetTodoListEndpoint
{
    public static RouteHandlerBuilder MapGetTodoListEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/todo-lists/{todoListId:guid}", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoLists")
            .WithName("GetTodoList")
            .WithSummary("Get Todo List")
            .WithDescription("Retrieves a list the current user belongs to.")
            .Produces<TodoListDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetTodoListQuery(userId, todoListId), cancellationToken).ToOk();
    }
}
