namespace SharedTodo.Users.Data;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=UsersDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new UsersDbContext(optionsBuilder.Options);
    }
}
