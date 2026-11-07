using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoListInvites;

public record RevokeTodoListInviteCommand(Guid UserId, Guid TodoListId, Guid InviteId) : ICommand<Result>;

public class RevokeTodoListInviteValidator : AbstractValidator<RevokeTodoListInviteCommand>
{
    public RevokeTodoListInviteValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();

        RuleFor(x => x.InviteId)
            .NotEmpty();
    }
}

public class RevokeTodoListInviteHandler(TodosDbContext dbContext) : ICommandHandler<RevokeTodoListInviteCommand, Result>
{
    public async ValueTask<Result> Handle(RevokeTodoListInviteCommand command, CancellationToken cancellationToken)
    {
        var membership = await dbContext.TodoListMemberships
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == command.UserId && x.TodoListId == command.TodoListId, cancellationToken);

        if (membership is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        if (!membership.IsOwner)
        {
            return Result.Forbidden("Only the list owner can revoke invites.");
        }

        var invite = await dbContext.TodoListInvites
            .SingleOrDefaultAsync(x => x.Id == command.InviteId && x.TodoListId == command.TodoListId, cancellationToken);

        if (invite is null)
        {
            return Result.NotFound("Invite not found.");
        }

        invite.Revoke();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public static class RevokeTodoListInviteEndpoint
{
    public static RouteHandlerBuilder MapRevokeTodoListInviteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapDelete("/todo-lists/{todoListId:guid}/invites/{inviteId:guid}", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoListInvites")
            .WithName("RevokeTodoListInvite")
            .WithSummary("Revoke Todo List Invite")
            .WithDescription("Revokes an invite so its token can no longer be used. People who already joined keep their membership. Revoking an already revoked invite has no effect. Only the owner can revoke it.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, Guid inviteId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new RevokeTodoListInviteCommand(userId, todoListId, inviteId), cancellationToken).ToNoContent();
    }
}
