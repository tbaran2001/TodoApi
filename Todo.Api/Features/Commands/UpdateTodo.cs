using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;
using Todo.Api.Dtos;
using Microsoft.Extensions.Options;
using Todo.Api.Common.Options;

namespace Todo.Api.Features.Commands;

public record UpdateTodoCommand(Guid Id, string Title, string Description, DateTimeOffset DueDate)
    : ICommand<UpdateTodoResult>;

public record UpdateTodoResult(TodoTaskDto Todo);

public record UpdateTodoRequestDto(string Title, string Description, DateTimeOffset DueDate);

public record UpdateTodoResponseDto(TodoTaskDto Todo);

public class UpdateTodoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/todos/{id:guid}", async (Guid id, UpdateTodoRequestDto request, ISender mediator) =>
        {
            var command = new UpdateTodoCommand(id, request.Title, request.Description, request.DueDate);

            var result = await mediator.Send(command);

            var response = result.Adapt<UpdateTodoResponseDto>();

            return Results.Ok(response);
        });
    }
}

public class UpdateTodoHandler(ITodosRepository todosRepository) : ICommandHandler<UpdateTodoCommand, UpdateTodoResult>
{
    public async Task<UpdateTodoResult> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todosRepository.GetTodoByIdAsync(request.Id, cancellationToken);
        if (todo == null)
            throw new TodoTaskNotFoundException(request.Id);

        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.DueDate = request.DueDate;

        var dto = todo.Adapt<TodoTaskDto>();
        return new UpdateTodoResult(dto);
    }
}

public class UpdateTodoValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoValidator(IOptions<TodoValidationOptions> options, TimeProvider timeProvider)
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title must not be empty or whitespace")
            .MaximumLength(options.Value.TitleMaxLength);

        RuleFor(x => x.Description)
            .NotNull()
            .MaximumLength(options.Value.DescriptionMaxLength);

        RuleFor(x => x.DueDate)
            .NotEmpty()
            .Must(d => d >= timeProvider.GetUtcNow())
            .WithMessage("DueDate must be now or in the future");
    }
}