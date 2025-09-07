using Todo.Api.Models;

namespace Todo.Api.Data.Repos;

public interface ITodosRepository
{
    Task<IEnumerable<TodoTask>> GetAllTodosAsync(CancellationToken cancellationToken);
    Task<(IEnumerable<TodoTask> Todos, int TotalCount)> GetTodosPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<TodoTask?> GetTodoByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TodoTask?> GetTodoByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<TodoTask>> GetIncomingTodosAsync(DateTimeOffset from, DateTimeOffset to,
        CancellationToken cancellationToken);
    Task AddTodoAsync(TodoTask todo, CancellationToken cancellationToken);
    void RemoveTodoAsync(TodoTask todo, CancellationToken cancellationToken);
}