using SharedTodo.Users.Identity;

namespace SharedTodo.Users.Jwt;

public interface ITokenService
{
    TokenDto GenerateToken(ApplicationUser user);
}

public record TokenDto(string AccessToken);
