namespace CrewTasks.Todos.Contracts.Events;

public record TodoListMemberJoinedEvent(Guid TodoListId, string TodoListName, Guid OwnerId, Guid MemberId) : INotification;
