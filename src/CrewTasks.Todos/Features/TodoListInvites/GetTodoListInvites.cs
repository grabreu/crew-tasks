using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoListInvites;

public record GetTodoListInvitesQuery(Guid UserId, Guid TodoListId) : IQuery<Result<IReadOnlyList<TodoListInviteDto>>>;

public class GetTodoListInvitesValidator : AbstractValidator<GetTodoListInvitesQuery>
{
    public GetTodoListInvitesValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();
    }
}

public class GetTodoListInvitesHandler(TodosDbContext dbContext) : IQueryHandler<GetTodoListInvitesQuery, Result<IReadOnlyList<TodoListInviteDto>>>
{
    public async ValueTask<Result<IReadOnlyList<TodoListInviteDto>>> Handle(GetTodoListInvitesQuery query, CancellationToken cancellationToken)
    {
        var membership = await dbContext.TodoListMemberships
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == query.UserId && x.TodoListId == query.TodoListId, cancellationToken);

        if (membership is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        if (!membership.IsOwner)
        {
            return Result.Forbidden("Only the list owner can view invites.");
        }

        var now = DateTimeOffset.UtcNow;

        IReadOnlyList<TodoListInviteDto> invites = await dbContext.TodoListInvites
            .AsNoTracking()
            .Where(x => x.TodoListId == query.TodoListId && x.RevokedAt == null && x.ExpiresAt > now)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new TodoListInviteDto(x.Id, x.TodoListId, x.Token, x.CreatedAt, x.ExpiresAt))
            .ToListAsync(cancellationToken);

        return Result.Success(invites);
    }
}

public static class GetTodoListInvitesEndpoint
{
    public static RouteHandlerBuilder MapGetTodoListInvitesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/todo-lists/{todoListId:guid}/invites", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoListInvites")
            .WithName("GetTodoListInvites")
            .WithSummary("Get Todo List Invites")
            .WithDescription("Retrieves the active invites (not revoked, not expired) of a list, oldest first. Only the owner can view them.")
            .Produces<IReadOnlyList<TodoListInviteDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetTodoListInvitesQuery(userId, todoListId), cancellationToken).ToOk();
    }
}
