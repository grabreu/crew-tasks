using CrewTasks.Users.Identity;

namespace CrewTasks.Users.Features;

public record CurrentUserDto(Guid Id, string Email, string? DisplayName, bool IsOnboarded);

public record GetCurrentUserQuery(Guid UserId) : IQuery<Result<CurrentUserDto>>;

public class GetCurrentUserValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}

public class GetCurrentUserHandler(UserManager<ApplicationUser> userManager) : IQueryHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    public async ValueTask<Result<CurrentUserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.UserId.ToString());

        if (user is null)
        {
            return Result.NotFound("User not found.");
        }

        return new CurrentUserDto(user.Id, user.Email!, user.DisplayName, user.OnboardedAt is not null);
    }
}

public static class GetCurrentUserEndpoint
{
    public static RouteHandlerBuilder MapGetCurrentUserEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGet("/users/me", HandleAsync)
            .RequireAuthorization()
            .WithTags("Users")
            .WithName("GetCurrentUser")
            .WithSummary("Get Current User")
            .WithDescription("Retrieves the currently authenticated user.")
            .Produces<CurrentUserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new GetCurrentUserQuery(userId), cancellationToken).ToOk();
    }
}
