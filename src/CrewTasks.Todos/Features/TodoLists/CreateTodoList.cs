using CrewTasks.Todos.Data;
using CrewTasks.Todos.Entities;

namespace CrewTasks.Todos.Features.TodoLists;

public record TodoListDto(Guid Id, string Name, DateTimeOffset CreatedAt);

public record CreateTodoListCommand(Guid UserId, string Name) : ICommand<Result<TodoListDto>>;

public class CreateTodoListValidator : AbstractValidator<CreateTodoListCommand>
{
    public CreateTodoListValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}

public class CreateTodoListHandler(TodosDbContext dbContext) : ICommandHandler<CreateTodoListCommand, Result<TodoListDto>>
{
    public async ValueTask<Result<TodoListDto>> Handle(CreateTodoListCommand command, CancellationToken cancellationToken)
    {
        var todoList = new TodoList(command.Name, command.UserId);

        dbContext.TodoLists.Add(todoList);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TodoListDto(todoList.Id, todoList.Name, todoList.CreatedAt);
    }
}

public static class CreateTodoListEndpoint
{
    public static RouteHandlerBuilder MapCreateTodoListEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost("/todo-lists", HandleAsync)
            .RequireAuthorization()
            .WithTags("TodoLists")
            .WithName("CreateTodoList")
            .WithSummary("Create Todo List")
            .WithDescription("Creates a list owned by the current user.")
            .Produces<TodoListDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static ValueTask<IResult> HandleAsync(CreateTodoListRequest request, ClaimsPrincipal principal, ISender sender, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return sender.Send(new CreateTodoListCommand(userId, request.Name), cancellationToken).ToCreated(list => $"/todo-lists/{list.Id}");
    }

    public record CreateTodoListRequest(string Name);
}
