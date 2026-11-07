using CrewTasks.Users.Contracts;
using CrewTasks.Users.Data;

namespace CrewTasks.Users.Identity;

public class UserDirectory(UsersDbContext dbContext) : IUserDirectory
{
    public async Task<IReadOnlyDictionary<Guid, UserInfo>> GetUsersAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().ToList();

        return await dbContext.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new UserInfo(x.Id, x.DisplayName, x.Email))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
    }
}
