using CrewTasks.Users.Identity;
using CrewTasks.Users.Jwt;

namespace CrewTasks.Users.Features;

public record CompleteEmailSignInCommand(string Email, string Code) : ICommand<Result<TokenDto>>;

public class CompleteEmailSignInValidator : AbstractValidator<CompleteEmailSignInCommand>
{
    public CompleteEmailSignInValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{6}$");
    }
}

public class CompleteEmailSignInHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService) : ICommandHandler<CompleteEmailSignInCommand, Result<TokenDto>>
{
    public async ValueTask<Result<TokenDto>> Handle(CompleteEmailSignInCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            return Result.Unauthorized("Invalid or expired code.");
        }

        if (!await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, StartEmailSignInHandler.Purpose, command.Code))
        {
            await userManager.AccessFailedAsync(user);

            return Result.Unauthorized("Invalid or expired code.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        user.EmailConfirmed = true;

        var stampResult = await userManager.UpdateSecurityStampAsync(user);

        if (!stampResult.Succeeded)
        {
            return Result.Unauthorized("Invalid or expired code.");
        }

        return tokenService.GenerateToken(user);
    }
}

public static class CompleteEmailSignInEndpoint
{
    public static RouteHandlerBuilder MapCompleteEmailSignInEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/auth/email/sign-in/complete", HandleAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .WithName("CompleteEmailSignIn")
            .WithSummary("Complete Email Sign-In")
            .WithDescription("Completes the email sign-in process using a verification code.")
            .Produces<TokenDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static ValueTask<IResult> HandleAsync(CompleteEmailSignInRequest request, ISender sender, CancellationToken cancellationToken)
    {
        return sender.Send(new CompleteEmailSignInCommand(request.Email, request.Code), cancellationToken).ToOk();
    }

    public record CompleteEmailSignInRequest(string Email, string Code);
}
