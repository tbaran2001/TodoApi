using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Data.Repos;
using Todo.Api.Models;
using Todo.Api.Dtos;
using Microsoft.Extensions.Options;
using Todo.Api.Common.Options;

namespace Todo.Api.Features.Commands;

public record CreateTodoCommand(string Title, string Description, DateTimeOffset DueDate) : ICommand<CreateTodoResult>;

public record CreateTodoResult(TodoTaskDto Todo);

public record CreateTodoRequestDto(string Title, string Description, DateTimeOffset DueDate);

public record CreateTodoResponseDto(TodoTaskDto Todo);

public class CreateTodoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/todos", async (CreateTodoRequestDto request, ISender mediator) =>
        {
            var command = new CreateTodoCommand(request.Title, request.Description, request.DueDate);

            var result = await mediator.Send(command);

            var response = result.Adapt<CreateTodoResponseDto>();

            return Results.CreatedAtRoute("GetTodoById", new { id = response.Todo.Id }, response);
        });
    }
}

public class CreateTodoHandler(ITodosRepository todosRepository) : ICommandHandler<CreateTodoCommand, CreateTodoResult>
{
    public async Task<CreateTodoResult> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var entity = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            CompletionPercentage = 0
        };

        await todosRepository.AddTodoAsync(entity, cancellationToken);

        var dto = entity.Adapt<TodoTaskDto>();
        return new CreateTodoResult(dto);
    }
}

public class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator(IOptions<Todo.Api.Common.Options.TodoValidationOptions> options,
        TimeProvider timeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title must not be empty or whitespace")
            .MaximumLength(options.Value.TitleMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty()
            .Must(desc => !string.IsNullOrWhiteSpace(desc))
            .WithMessage("Description must not be empty or whitespace")
            .MaximumLength(options.Value.DescriptionMaxLength);

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .Must(d => d >= timeProvider.GetUtcNow())
            .WithMessage("DueDate must be now or in the future");
    }
}