namespace CrewTasks.Users.Contracts;

public record UserInfo(Guid Id, string? DisplayName, string? Email);

public interface IUserDirectory
{
    Task<IReadOnlyDictionary<Guid, UserInfo>> GetUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
}
