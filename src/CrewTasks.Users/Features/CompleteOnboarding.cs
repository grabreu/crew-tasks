using CrewTasks.Users.Identity;

namespace CrewTasks.Users.Features;

public record CompleteOnboardingCommand(Guid UserId, string DisplayName) : ICommand<Result>;

public class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class CompleteOnboardingHandler(UserManager<ApplicationUser> userManager) : ICommandHandler<CompleteOnboardingCommand, Result>
{
    public async ValueTask<Result> Handle(CompleteOnboardingCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            return Result.NotFound("User not found.");
        }

        if (user.OnboardedAt is not null)
        {
            return Result.Conflict("User has already completed onboarding.");
        }

        user.DisplayName = command.DisplayName;
        user.OnboardedAt = DateTimeOffset.UtcNow;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return updateResult.ToErrors();
        }

        return Result.Success();
    }
}

public static class CompleteOnboardingEndpoint
{
    public static RouteHandlerBuilder MapCompleteOnboardingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/users/me/onboarding", HandleAsync)
            .RequireAuthorization()
            .WithTags("Users")
            .WithName("CompleteOnboarding")
            .WithSummary("Complete Onboarding")
            .WithDescription("Completes the onboarding process for the current user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static ValueTask<IResult> HandleAsync(CompleteOnboardingRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new CompleteOnboardingCommand(userId, request.DisplayName), cancellationToken).ToNoContent();
    }

    public record CompleteOnboardingRequest(string DisplayName);
}
