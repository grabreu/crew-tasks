using CrewTasks.Users.Identity;

namespace CrewTasks.Users.Jwt;

public class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public TokenDto GenerateToken(ApplicationUser user)
    {
        var subject = new ClaimsIdentity(
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!)
        ]);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.Value.AccessTokenExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Issuer = options.Value.Issuer,
            Audience = options.Value.Audience,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials,
        };

        var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenDto(accessToken);
    }
}
