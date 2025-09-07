using Microsoft.EntityFrameworkCore;
using Todo.Api.Models;

namespace Todo.Api.Data.Repos;

public class TodosRepository(ApplicationDbContext dbContext) : ITodosRepository
{
    public async Task<IEnumerable<TodoTask>> GetAllTodosAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Todos
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<TodoTask> Todos, int TotalCount)> GetTodosPagedAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Todos.AsNoTracking();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var todos = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (todos, totalCount);
    }

    public async Task<TodoTask?> GetTodoByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TodoTask?> GetTodoByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Todos
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TodoTask>> GetIncomingTodosAsync(DateTimeOffset from, DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await dbContext.Todos
            .AsNoTracking()
            .Where(t => t.DueDate >= from && t.DueDate <= to)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddTodoAsync(TodoTask todo, CancellationToken cancellationToken)
    {
        await dbContext.Todos.AddAsync(todo, cancellationToken);
    }

    public void RemoveTodoAsync(TodoTask todo, CancellationToken cancellationToken)
    {
        dbContext.Todos.Remove(todo);
    }
}