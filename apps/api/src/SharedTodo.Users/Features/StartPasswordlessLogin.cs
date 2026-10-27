using SharedTodo.Users.Contracts.Events;
using SharedTodo.Users.Identity;

namespace SharedTodo.Users.Features;

public record StartPasswordlessLoginCommand(string Email) : ICommand<Result>;

public class StartPasswordlessLoginValidator : AbstractValidator<StartPasswordlessLoginCommand>
{
    public StartPasswordlessLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();
    }
}

public class StartPasswordlessLoginHandler(UserManager<ApplicationUser> userManager, IPublisher publisher) : ICommandHandler<StartPasswordlessLoginCommand, Result>
{
    public const string Purpose = "PasswordlessLogin";

    public async ValueTask<Result> Handle(StartPasswordlessLoginCommand command, CancellationToken cancellationToken)
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

        await publisher.Publish(new PasswordlessLoginStartedEvent(user.Email!, code), cancellationToken);

        return Result.Success();
    }
}

public static class StartPasswordlessLoginEndpoint
{
    public static RouteHandlerBuilder MapStartPasswordlessLoginEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/auth/login/passwordless", HandleAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .WithName("StartPasswordlessLogin")
            .WithSummary("Start Passwordless Login")
            .WithDescription("Initiates the passwordless login process for a user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();
    }

    private static ValueTask<IResult> HandleAsync(StartPasswordlessLoginRequest request, ISender sender, CancellationToken cancellationToken)
    {
        return sender.Send(new StartPasswordlessLoginCommand(request.Email), cancellationToken).ToNoContent();
    }

    public record StartPasswordlessLoginRequest(string Email);
}
