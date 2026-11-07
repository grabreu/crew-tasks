using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoListInvites;

public record TodoListInviteDto(Guid Id, Guid TodoListId, string Token, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt);

public record CreateTodoListInviteCommand(Guid UserId, Guid TodoListId) : ICommand<Result<TodoListInviteDto>>;

public class CreateTodoListInviteValidator : AbstractValidator<CreateTodoListInviteCommand>
{
    public CreateTodoListInviteValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();
    }
}

public class CreateTodoListInviteHandler(TodosDbContext dbContext) : ICommandHandler<CreateTodoListInviteCommand, Result<TodoListInviteDto>>
{
    public async ValueTask<Result<TodoListInviteDto>> Handle(CreateTodoListInviteCommand command, CancellationToken cancellationToken)
    {
        var membership = await dbContext.TodoListMemberships
            .Include(x => x.TodoList)
            .SingleOrDefaultAsync(x => x.UserId == command.UserId && x.TodoListId == command.TodoListId, cancellationToken);

        if (membership is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        if (!membership.IsOwner)
        {
            return Result.Forbidden("Only the list owner can create invites.");
        }

        var invite = membership.TodoList.AddInvite(command.UserId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoListInviteDto(invite.Id, invite.TodoListId, invite.Token, invite.CreatedAt, invite.ExpiresAt);
    }
}

public static class CreateTodoListInviteEndpoint
{
    public static RouteHandlerBuilder MapCreateTodoListInviteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/todo-lists/{todoListId:guid}/invites", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoListInvites")
            .WithName("CreateTodoListInvite")
            .WithSummary("Create Todo List Invite")
            .WithDescription("Creates an invite link token for a list, valid for 7 days and usable by several people until it expires or is revoked. Only the owner can create it.")
            .Produces<TodoListInviteDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new CreateTodoListInviteCommand(userId, todoListId), cancellationToken).ToCreated(invite => $"/todo-lists/{invite.TodoListId}/invites");
    }
}
