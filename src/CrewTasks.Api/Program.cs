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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapUsersModuleEndpoints();

if (app.Environment.IsDevelopment())
{
    await app.EnsureUsersModuleDatabaseAsync();
}

await app.RunAsync();
