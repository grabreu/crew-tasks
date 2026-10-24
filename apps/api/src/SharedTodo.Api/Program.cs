using SharedTodo.Api.Behaviors;
using SharedTodo.Api.ExceptionHandling;
using SharedTodo.Api.OpenApi;
using SharedTodo.Notifications;
using SharedTodo.ServiceDefaults;
using SharedTodo.Users;

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
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

app.UseCors();

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
