using CrewTasks.Users.Google;
using CrewTasks.Users.Identity;
using CrewTasks.Users.Jwt;

namespace CrewTasks.Users.Features;

public record SignInWithGoogleCommand(string IdToken) : ICommand<Result<TokenDto>>;

public class SignInWithGoogleValidator : AbstractValidator<SignInWithGoogleCommand>
{
    public SignInWithGoogleValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty();
    }
}

public class SignInWithGoogleHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService, IGoogleService googleService) : ICommandHandler<SignInWithGoogleCommand, Result<TokenDto>>
{
    private const string LoginProvider = "Google";

    public async ValueTask<Result<TokenDto>> Handle(SignInWithGoogleCommand command, CancellationToken cancellationToken)
    {
        var googleIdentity = await googleService.ValidateIdTokenAsync(command.IdToken, cancellationToken);

        if (googleIdentity is null)
        {
            return Result.Unauthorized("Invalid Google ID token.");
        }

        var user = await userManager.FindByLoginAsync(LoginProvider, googleIdentity.Subject);

        if (user is not null)
        {
            return tokenService.GenerateToken(user);
        }

        user = await userManager.FindByEmailAsync(googleIdentity.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = googleIdentity.Email,
                Email = googleIdentity.Email
            };

            var createResult = await userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                return createResult.ToErrors();
            }
        }

        user.EmailConfirmed = true;

        var addLoginResult = await userManager.AddLoginAsync(user, new UserLoginInfo(LoginProvider, googleIdentity.Subject, LoginProvider));

        if (!addLoginResult.Succeeded)
        {
            return addLoginResult.ToErrors();
        }

        return tokenService.GenerateToken(user);
    }
}

public record SignInWithGoogleRequest(string IdToken);

public static class SignInWithGoogleEndpoint
{
    public static RouteHandlerBuilder MapSignInWithGoogleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/auth/google/sign-in", HandleAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .WithName("SignInWithGoogle")
            .WithSummary("Sign In with Google")
            .WithDescription("Authenticates a user using a Google ID token.")
            .Produces<TokenDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static ValueTask<IResult> HandleAsync(SignInWithGoogleRequest request, ISender sender, CancellationToken cancellationToken)
    {
        return sender.Send(new SignInWithGoogleCommand(request.IdToken), cancellationToken).ToOk();
    }
}
