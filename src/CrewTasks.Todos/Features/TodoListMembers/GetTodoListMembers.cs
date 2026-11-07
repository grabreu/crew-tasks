using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoListMembers;

public record TodoListMemberDto(Guid UserId, string Role, DateTimeOffset JoinedAt);

public record GetTodoListMembersQuery(Guid UserId, Guid TodoListId) : IQuery<Result<IReadOnlyList<TodoListMemberDto>>>;

public class GetTodoListMembersValidator : AbstractValidator<GetTodoListMembersQuery>
{
    public GetTodoListMembersValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();
    }
}

public class GetTodoListMembersHandler(TodosDbContext dbContext) : IQueryHandler<GetTodoListMembersQuery, Result<IReadOnlyList<TodoListMemberDto>>>
{
    public async ValueTask<Result<IReadOnlyList<TodoListMemberDto>>> Handle(GetTodoListMembersQuery query, CancellationToken cancellationToken)
    {
        var isMember = await dbContext.TodoListMemberships
            .AnyAsync(x => x.UserId == query.UserId && x.TodoListId == query.TodoListId, cancellationToken);

        if (!isMember)
        {
            return Result.NotFound("Todo list not found.");
        }

        IReadOnlyList<TodoListMemberDto> members = await dbContext.TodoListMemberships
            .AsNoTracking()
            .Where(x => x.TodoListId == query.TodoListId)
            .OrderBy(x => x.JoinedAt)
            .ThenBy(x => x.UserId)
            .Select(x => new TodoListMemberDto(x.UserId, x.Role.ToString(), x.JoinedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(members);
    }
}

public static class GetTodoListMembersEndpoint
{
    public static RouteHandlerBuilder MapGetTodoListMembersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/todo-lists/{todoListId:guid}/members", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoListMembers")
            .WithName("GetTodoListMembers")
            .WithSummary("Get Todo List Members")
            .WithDescription("Retrieves the members of a list the current user belongs to, in the order they joined.")
            .Produces<IReadOnlyList<TodoListMemberDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetTodoListMembersQuery(userId, todoListId), cancellationToken).ToOk();
    }
}
