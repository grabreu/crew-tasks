using SharedTodo.Users.Identity;

namespace SharedTodo.Users.Data;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : IdentityUserContext<ApplicationUser, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("Users");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
