using SharedTodo.Users.Google;
using SharedTodo.Users.Identity;
using SharedTodo.Users.Jwt;

namespace SharedTodo.Users.Features;

public record LoginWithGoogleCommand(string IdToken) : ICommand<Result<TokenDto>>;

public class LoginWithGoogleValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty();
    }
}

public class LoginWithGoogleHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService, IGoogleService googleService) : ICommandHandler<LoginWithGoogleCommand, Result<TokenDto>>
{
    private const string LoginProvider = "Google";

    public async ValueTask<Result<TokenDto>> Handle(LoginWithGoogleCommand command, CancellationToken cancellationToken)
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

public record LoginWithGoogleRequest(string IdToken);

public static class LoginWithGoogleEndpoint
{
    public static RouteHandlerBuilder MapLoginWithGoogleEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/auth/login/google", HandleAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .WithName("LoginWithGoogle")
            .WithSummary("Login with Google")
            .WithDescription("Authenticates a user using a Google ID token.")
            .Produces<TokenDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static ValueTask<IResult> HandleAsync(LoginWithGoogleRequest request, ISender sender, CancellationToken cancellationToken)
    {
        return sender.Send(new LoginWithGoogleCommand(request.IdToken), cancellationToken).ToOk();
    }
}
