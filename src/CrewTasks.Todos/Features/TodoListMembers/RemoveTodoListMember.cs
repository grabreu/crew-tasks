using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoListMembers;

public record RemoveTodoListMemberCommand(Guid UserId, Guid TodoListId, Guid MemberId) : ICommand<Result>;

public class RemoveTodoListMemberValidator : AbstractValidator<RemoveTodoListMemberCommand>
{
    public RemoveTodoListMemberValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();

        RuleFor(x => x.MemberId)
            .NotEmpty();
    }
}

public class RemoveTodoListMemberHandler(TodosDbContext dbContext) : ICommandHandler<RemoveTodoListMemberCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveTodoListMemberCommand command, CancellationToken cancellationToken)
    {
        var memberships = await dbContext.TodoListMemberships
            .Where(x => x.TodoListId == command.TodoListId && (x.UserId == command.UserId || x.UserId == command.MemberId))
            .ToListAsync(cancellationToken);

        var caller = memberships.SingleOrDefault(x => x.UserId == command.UserId);

        if (caller is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        if (command.MemberId == command.UserId)
        {
            if (caller.IsOwner)
            {
                return Result.Conflict("The owner cannot leave the list. Delete the list instead.");
            }

            dbContext.TodoListMemberships.Remove(caller);
        }
        else
        {
            if (!caller.IsOwner)
            {
                return Result.Forbidden("Only the list owner can remove other members.");
            }

            var target = memberships.SingleOrDefault(x => x.UserId == command.MemberId);

            if (target is not null)
            {
                dbContext.TodoListMemberships.Remove(target);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public static class RemoveTodoListMemberEndpoint
{
    public static RouteHandlerBuilder MapRemoveTodoListMemberEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapDelete("/todo-lists/{todoListId:guid}/members/{userId:guid}", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoListMembers")
            .WithName("RemoveTodoListMember")
            .WithSummary("Remove Todo List Member")
            .WithDescription("Removes a member from a list. A member removes themselves to leave the list; the owner removes anyone else. The owner cannot leave (409): delete the list instead. Removing someone who is not a member has no effect. A removed person can join again through an active invite.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static ValueTask<IResult> HandleAsync(Guid todoListId, Guid userId, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var callerId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new RemoveTodoListMemberCommand(callerId, todoListId, userId), cancellationToken).ToNoContent();
    }
}
