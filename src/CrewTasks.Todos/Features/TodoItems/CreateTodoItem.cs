using CrewTasks.Todos.Data;

namespace CrewTasks.Todos.Features.TodoItems;

public record TodoItemDto(Guid Id, Guid TodoListId, string Title, bool IsCompleted, DateTimeOffset CreatedAt);

public record CreateTodoItemCommand(Guid UserId, Guid TodoListId, string Title) : ICommand<Result<TodoItemDto>>;

public class CreateTodoItemValidator : AbstractValidator<CreateTodoItemCommand>
{
    public CreateTodoItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.TodoListId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public class CreateTodoItemHandler(TodosDbContext dbContext) : ICommandHandler<CreateTodoItemCommand, Result<TodoItemDto>>
{
    public async ValueTask<Result<TodoItemDto>> Handle(CreateTodoItemCommand command, CancellationToken cancellationToken)
    {
        var todoList = await dbContext.TodoListMemberships
            .Where(x => x.UserId == command.UserId && x.TodoListId == command.TodoListId)
            .Select(x => x.TodoList)
            .SingleOrDefaultAsync(cancellationToken);

        if (todoList is null)
        {
            return Result.NotFound("Todo list not found.");
        }

        var todoItem = todoList.AddItem(command.Title);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoItemDto(todoItem.Id, todoItem.TodoListId, todoItem.Title, todoItem.IsCompleted, todoItem.CreatedAt);
    }
}

public static class CreateTodoItemEndpoint
{
    public static RouteHandlerBuilder MapCreateTodoItemEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/todo-items", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoItems")
            .WithName("CreateTodoItem")
            .WithSummary("Create Todo Item")
            .WithDescription("Creates an item in a list the current user belongs to.")
            .Produces<TodoItemDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static ValueTask<IResult> HandleAsync(CreateTodoItemRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new CreateTodoItemCommand(userId, request.TodoListId, request.Title), cancellationToken).ToCreated(item => $"/todo-items/{item.Id}");
    }

    public record CreateTodoItemRequest(Guid TodoListId, string Title);
}
