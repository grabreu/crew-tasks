namespace SharedTodo.Users.Common;

public interface ITokenService
{
    TokenDto GenerateToken(ApplicationUser user);
}
