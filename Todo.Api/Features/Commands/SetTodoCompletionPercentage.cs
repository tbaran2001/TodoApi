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

public record SetTodoCompletionPercentageCommand(Guid Id, int CompletionPercentage)
    : ICommand<SetTodoCompletionPercentageResult>;

public record SetTodoCompletionPercentageResult(TodoTaskDto Todo);

public record SetTodoCompletionPercentageRequestDto(int CompletionPercentage);

public record SetTodoCompletionPercentageResponseDto(TodoTaskDto Todo);

public class SetTodoCompletionPercentageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/todos/{id:guid}/completion-percentage",
            async (Guid id, SetTodoCompletionPercentageRequestDto request, ISender mediator) =>
            {
                var command = new SetTodoCompletionPercentageCommand(id, request.CompletionPercentage);

                var result = await mediator.Send(command);

                var response = result.Adapt<SetTodoCompletionPercentageResponseDto>();

                return Results.Ok(response);
            });
    }
}

public class SetTodoCompletionPercentageHandler(ITodosRepository todosRepository)
    : ICommandHandler<SetTodoCompletionPercentageCommand, SetTodoCompletionPercentageResult>
{
    public async Task<SetTodoCompletionPercentageResult> Handle(SetTodoCompletionPercentageCommand request,
        CancellationToken cancellationToken)
    {
        var todo = await todosRepository.GetTodoByIdAsync(request.Id, cancellationToken);
        if (todo == null)
            throw new TodoTaskNotFoundException(request.Id);

        todo.CompletionPercentage = request.CompletionPercentage;

        var dto = todo.Adapt<TodoTaskDto>();
        return new SetTodoCompletionPercentageResult(dto);
    }
}

public class SetTodoCompletionPercentageValidator : AbstractValidator<SetTodoCompletionPercentageCommand>
{
    public SetTodoCompletionPercentageValidator(IOptions<TodoValidationOptions> options)
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.CompletionPercentage)
            .InclusiveBetween(options.Value.CompletionPercentageMin, options.Value.CompletionPercentageMax);
    }
}