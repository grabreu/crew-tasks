using SharedTodo.Api.ExceptionHandling;
using SharedTodo.Api.OpenApi;
using SharedTodo.ServiceDefaults;
using SharedTodo.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<OpenApiVersioningTransformer>();
});

builder.AddUsersModuleServices();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (app.Environment.IsDevelopment())
{
    await app.EnsureUsersModuleDatabaseAsync();
}

await app.RunAsync();
