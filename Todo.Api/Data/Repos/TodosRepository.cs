using Microsoft.EntityFrameworkCore;
using Todo.Api.Models;

namespace Todo.Api.Data.Repos;

public class TodosRepository(ApplicationDbContext dbContext) : ITodosRepository
{
    public async Task<IEnumerable<TodoTask>> GetAllTodosAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Todos.ToListAsync(cancellationToken);
    }

    public async Task<TodoTask?> GetTodoByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TodoTask>> GetIncomingTodosAsync(DateTimeOffset from, DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await dbContext.Todos
            .Where(t => t.DueDate >= from && t.DueDate <= to)
            .ToListAsync(cancellationToken);
    }

    public async Task AddTodoAsync(TodoTask todo, CancellationToken cancellationToken)
    {
        await dbContext.Todos.AddAsync(todo);
    }

    public void RemoveTodoAsync(TodoTask todo, CancellationToken cancellationToken)
    {
        dbContext.Todos.Remove(todo);
    }
}