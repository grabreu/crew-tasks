using CrewTasks.Users.Contracts;
using CrewTasks.Users.Data;
using CrewTasks.Users.Features;
using CrewTasks.Users.Identity;
using CrewTasks.Users.Jwt;

namespace CrewTasks.Users;

public static class UsersModuleExtensions
{
    public const string Schema = "Users";
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public static IHostApplicationBuilder AddUsersModuleServices(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<UsersDbContext>("UsersDb", configureDbContextOptions: options => options.UseSqlServer(sql => sql.MigrationsHistoryTable(MigrationsHistoryTable, Schema)));

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, options) =>
            {
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = options.Value.Issuer,
                    ValidAudience = options.Value.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddSingleton<ITokenService, JwtTokenService>();

        builder.Services.AddScoped<IUserDirectory, UserDirectory>();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return builder;
    }

    public static IEndpointRouteBuilder MapUsersModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapStartEmailSignInEndpoint();
        endpoints.MapCompleteEmailSignInEndpoint();
        endpoints.MapGetCurrentUserEndpoint();
        endpoints.MapCompleteOnboardingEndpoint();

        return endpoints;
    }

    public static async Task EnsureUsersModuleDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await context.Database.MigrateAsync();
    }
}
