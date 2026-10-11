using SharedTodo.Users.Contracts.Events;
using SharedTodo.Users.Identity;

namespace SharedTodo.Users.Features;

public record StartEmailSignInCommand(string Email) : ICommand<Result>;

public class StartEmailSignInValidator : AbstractValidator<StartEmailSignInCommand>
{
    public StartEmailSignInValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();
    }
}

public class StartEmailSignInHandler(UserManager<ApplicationUser> userManager, IPublisher publisher) : ICommandHandler<StartEmailSignInCommand, Result>
{
    public const string Purpose = "PasswordlessLogin";

    public async ValueTask<Result> Handle(StartEmailSignInCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = command.Email,
                Email = command.Email
            };

            var createResult = await userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return createResult.ToErrors();
            }
        }

        var code = await userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, Purpose);

        await publisher.Publish(new EmailSignInStartedEvent(user.Email!, code), cancellationToken);

        return Result.Success();
    }
}

public static class StartEmailSignInEndpoint
{
    public static RouteHandlerBuilder MapStartEmailSignInEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/auth/email/sign-in", HandleAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .WithName("StartEmailSignIn")
            .WithSummary("Start Email Sign-In")
            .WithDescription("Initiates the email sign-in process for a user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();
    }

    private static ValueTask<IResult> HandleAsync(StartEmailSignInRequest request, ISender sender, CancellationToken cancellationToken)
    {
        return sender.Send(new StartEmailSignInCommand(request.Email), cancellationToken).ToNoContent();
    }

    public record StartEmailSignInRequest(string Email);
}
