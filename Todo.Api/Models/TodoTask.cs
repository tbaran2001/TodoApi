namespace Todo.Api.Models;

public class TodoTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset DueDate { get; set; }
    public int CompletionPercentage { get; set; }
}