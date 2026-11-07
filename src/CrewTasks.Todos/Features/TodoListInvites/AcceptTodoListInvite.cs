using CrewTasks.Todos.Data;
using CrewTasks.Todos.Features.TodoLists;

namespace CrewTasks.Todos.Features.TodoListInvites;

public record AcceptTodoListInviteCommand(Guid UserId, string Token) : ICommand<Result<TodoListDto>>;

public class AcceptTodoListInviteValidator : AbstractValidator<AcceptTodoListInviteCommand>
{
    public AcceptTodoListInviteValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(64);
    }
}

public class AcceptTodoListInviteHandler(TodosDbContext dbContext) : ICommandHandler<AcceptTodoListInviteCommand, Result<TodoListDto>>
{
    public async ValueTask<Result<TodoListDto>> Handle(AcceptTodoListInviteCommand command, CancellationToken cancellationToken)
    {
        var invite = await dbContext.TodoListInvites
            .Include(x => x.TodoList)
            .SingleOrDefaultAsync(x => x.Token == command.Token, cancellationToken);

        if (invite is null || !invite.IsUsable(DateTimeOffset.UtcNow))
        {
            return Result.NotFound("Invite not found.");
        }

        var isMember = await dbContext.TodoListMemberships
            .AnyAsync(x => x.UserId == command.UserId && x.TodoListId == invite.TodoListId, cancellationToken);

        if (!isMember)
        {
            invite.TodoList.AddMember(command.UserId);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new TodoListDto(invite.TodoList.Id, invite.TodoList.Name, invite.TodoList.CreatedAt);
    }
}

public static class AcceptTodoListInviteEndpoint
{
    public static RouteHandlerBuilder MapAcceptTodoListInviteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/todo-list-invites/{token}/accept", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoListInvites")
            .WithName("AcceptTodoListInvite")
            .WithSummary("Accept Todo List Invite")
            .WithDescription("Joins the list of an invite as a member. Accepting when already a member has no effect. An unknown, expired, or revoked token responds 404.")
            .Produces<TodoListDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(string token, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new AcceptTodoListInviteCommand(userId, token), cancellationToken).ToOk();
    }
}
