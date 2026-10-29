namespace CrewTasks.Users.Jwt;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; init; } = string.Empty;
    [Required] public string Audience { get; init; } = string.Empty;
    [Required, MinLength(32)] public string SecretKey { get; init; } = string.Empty;
    [Range(1, 1440)] public int AccessTokenExpirationMinutes { get; init; }
}
