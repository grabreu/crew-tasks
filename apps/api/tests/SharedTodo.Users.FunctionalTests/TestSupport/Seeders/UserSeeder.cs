using SharedTodo.Users.Common;

namespace SharedTodo.Users.FunctionalTests.TestSupport.Seeders;

public class UserSeeder(IServiceProvider services)
{
    public async Task<ApplicationUser> CreateAsync(string email)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user);

        result.Succeeded.ShouldBeTrue();

        return user;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await userManager.FindByEmailAsync(email);
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await userManager.GetLoginsAsync(user);
    }
}
