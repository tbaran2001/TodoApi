using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;
using Todo.Api.Dtos;

namespace Todo.Api.Features.Commands;

public record MarkTodoDoneCommand(Guid Id) : ICommand<MarkTodoDoneResult>;

public record MarkTodoDoneResult(TodoTaskDto Todo);

public record MarkTodoDoneResponseDto(TodoTaskDto Todo);

public class MarkTodoDoneEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/todos/{id:guid}/done", async (Guid id, ISender mediator) =>
        {
            var result = await mediator.Send(new MarkTodoDoneCommand(id));
            var response = result.Adapt<MarkTodoDoneResponseDto>();
            return Results.Ok(response);
        });
    }
}

public class MarkTodoDoneHandler(ITodosRepository todosRepository)
    : ICommandHandler<MarkTodoDoneCommand, MarkTodoDoneResult>
{
    public async Task<MarkTodoDoneResult> Handle(MarkTodoDoneCommand request, CancellationToken cancellationToken)
    {
        var todo = await todosRepository.GetTodoByIdForUpdateAsync(request.Id, cancellationToken);
        if (todo == null)
            throw new TodoTaskNotFoundException(request.Id);

        todo.CompletionPercentage = 100;

        var dto = todo.Adapt<TodoTaskDto>();
        return new MarkTodoDoneResult(dto);
    }
}

public class MarkTodoDoneValidator : AbstractValidator<MarkTodoDoneCommand>
{
    public MarkTodoDoneValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}