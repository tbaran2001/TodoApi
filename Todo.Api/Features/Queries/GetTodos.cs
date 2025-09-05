using Carter;
using Mapster;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Data.Repos;
using Todo.Api.Dtos;

namespace Todo.Api.Features.Queries;

public record GetTodosQuery() : IQuery<GetTodosResult>;

public record GetTodosResult(IEnumerable<TodoTaskDto> Todos);

public record GetTodosResponseDto(IEnumerable<TodoTaskDto> Todos);

public class GetTodosEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/todos", async (ISender mediator) =>
        {
            var query = new GetTodosQuery();

            var result = await mediator.Send(query);

            var response = result.Adapt<GetTodosResponseDto>();

            return Results.Ok(response);
        });
    }
}

public class GetTodosHandler(ITodosRepository todosRepository) : IQueryHandler<GetTodosQuery, GetTodosResult>
{
    public async Task<GetTodosResult> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await todosRepository.GetAllTodosAsync(cancellationToken);

        var todosDto = todos.Adapt<IEnumerable<TodoTaskDto>>();

        return new GetTodosResult(todosDto);
    }
}