namespace CrewTasks.Users.Google;

public class GoogleOptions
{
    public const string SectionName = "Authentication:Google";

    [Required] public string ClientId { get; init; } = string.Empty;
}
