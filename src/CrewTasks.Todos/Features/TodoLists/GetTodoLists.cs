using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoLists;

public record GetTodoListsQuery(Guid UserId) : IQuery<Result<IReadOnlyList<TodoListDto>>>;

public class GetTodoListsValidator : AbstractValidator<GetTodoListsQuery>
{
    public GetTodoListsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}

public class GetTodoListsHandler(TodosDbContext dbContext) : IQueryHandler<GetTodoListsQuery, Result<IReadOnlyList<TodoListDto>>>
{
    public async ValueTask<Result<IReadOnlyList<TodoListDto>>> Handle(GetTodoListsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<TodoListDto> todoLists = await dbContext.TodoListMemberships
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId)
            .Select(x => x.TodoList)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new TodoListDto(x.Id, x.Name, x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(todoLists);
    }
}

public static class GetTodoListsEndpoint
{
    public static RouteHandlerBuilder MapGetTodoListsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/todo-lists", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoLists")
            .WithName("GetTodoLists")
            .WithSummary("Get Todo Lists")
            .WithDescription("Retrieves the lists the current user belongs to, oldest first.")
            .Produces<IReadOnlyList<TodoListDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static ValueTask<IResult> HandleAsync(ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetTodoListsQuery(userId), cancellationToken).ToOk();
    }
}
