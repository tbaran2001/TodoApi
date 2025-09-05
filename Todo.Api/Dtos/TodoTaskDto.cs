namespace Todo.Api.Dtos;

public record TodoTaskDto(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset DueDate,
    int CompletionPercentage);