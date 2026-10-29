using CrewTasks.Users.Identity;

namespace CrewTasks.Users.Jwt;

public interface ITokenService
{
    TokenDto GenerateToken(ApplicationUser user);
}

public record TokenDto(string AccessToken);
