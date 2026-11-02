using CrewTasks.Api.Behaviors;
using CrewTasks.Api.ExceptionHandling;
using CrewTasks.Api.OpenApi;
using CrewTasks.Notifications;
using CrewTasks.ServiceDefaults;
using CrewTasks.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<OpenApiVersioningTransformer>();
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Servers = [];
        return Task.CompletedTask;
    });
    options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
});

builder.AddUsersModuleServices();
builder.AddNotificationsModuleServices();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.MapScalarApiReference(options => options.EnablePersistentAuthentication());
app.Map("/", () => Results.Redirect("/scalar"));

app.MapUsersModuleEndpoints();

await app.EnsureUsersModuleDatabaseAsync();

await app.RunAsync();
