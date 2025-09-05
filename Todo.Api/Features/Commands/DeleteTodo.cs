using Carter;
using FluentValidation;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;

namespace Todo.Api.Features.Commands;

public record DeleteTodoCommand(Guid Id) : ICommand;

public class DeleteTodoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/todos/{id:guid}", async (Guid id, ISender mediator) =>
        {
            await mediator.Send(new DeleteTodoCommand(id));
            return Results.NoContent();
        });
    }
}

public class DeleteTodoHandler(ITodosRepository todosRepository) : ICommandHandler<DeleteTodoCommand>
{
    public async Task<Unit> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await todosRepository.GetTodoByIdAsync(request.Id, cancellationToken);
        if (todo == null)
            throw new TodoTaskNotFoundException(request.Id);

        todosRepository.RemoveTodoAsync(todo, cancellationToken);
        return Unit.Value;
    }
}

public class DeleteTodoValidator : AbstractValidator<DeleteTodoCommand>
{
    public DeleteTodoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
