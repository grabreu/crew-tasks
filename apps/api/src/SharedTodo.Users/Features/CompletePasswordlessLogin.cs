using SharedTodo.Users.Identity;
using SharedTodo.Users.Jwt;

namespace SharedTodo.Users.Features;

public record CompletePasswordlessLoginCommand(string Email, string Code) : ICommand<Result<TokenDto>>;

public class CompletePasswordlessLoginValidator : AbstractValidator<CompletePasswordlessLoginCommand>
{
    public CompletePasswordlessLoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Code)
            .NotEmpty()
            .Matches(@"^\d{6}$");
    }
}

public class CompletePasswordlessLoginHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService) : ICommandHandler<CompletePasswordlessLoginCommand, Result<TokenDto>>
{
    public async ValueTask<Result<TokenDto>> Handle(CompletePasswordlessLoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            return Result.Unauthorized("Invalid or expired code.");
        }

        if (!await userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, StartPasswordlessLoginHandler.Purpose, command.Code))
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

public static class CompletePasswordlessLoginEndpoint
{
    public static RouteHandlerBuilder MapCompletePasswordlessLoginEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/auth/login/passwordless/complete", HandleAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .WithName("CompletePasswordlessLogin")
            .WithSummary("Complete Passwordless Login")
            .WithDescription("Completes the passwordless login process using a verification code.")
            .Produces<TokenDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static ValueTask<IResult> HandleAsync(CompletePasswordlessLoginRequest request, ISender sender, CancellationToken cancellationToken)
    {
        return sender.Send(new CompletePasswordlessLoginCommand(request.Email, request.Code), cancellationToken).ToOk();
    }

    public record CompletePasswordlessLoginRequest(string Email, string Code);
}
