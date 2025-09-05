using Carter;
using Mapster;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Data.Repos;
using Todo.Api.Dtos;
using FluentValidation;
using Todo.Api.Common.Exceptions;

namespace Todo.Api.Features.Queries;

public record GetTodoByIdQuery(Guid Id) : IQuery<GetTodoByIdResult>;
public record GetTodoByIdResult(TodoTaskDto Todo);
public record GetTodoByIdResponseDto(TodoTaskDto Todo);

public class GetTodoByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/todos/{id:guid}", async (Guid id, ISender mediator) =>
        {
            var query = new GetTodoByIdQuery(id);

            var result = await mediator.Send(query);

            var response = result.Adapt<GetTodoByIdResponseDto>();

            return Results.Ok(response);
        })
        .WithName("GetTodoById");
    }
}

public class GetTodoByIdHandler(ITodosRepository todosRepository) : IQueryHandler<GetTodoByIdQuery, GetTodoByIdResult>
{
    public async Task<GetTodoByIdResult> Handle(GetTodoByIdQuery query, CancellationToken cancellationToken)
    {
        var todo = await todosRepository.GetTodoByIdAsync(query.Id, cancellationToken);

        if (todo == null)
            throw new TodoTaskNotFoundException(query.Id);

        var todoDto = todo.Adapt<TodoTaskDto>();

        return new GetTodoByIdResult(todoDto);
    }
}

public class GetTodoByIdValidator : AbstractValidator<GetTodoByIdQuery>
{
    public GetTodoByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
