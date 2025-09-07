using Carter;
using Mapster;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Data.Repos;
using Todo.Api.Dtos;

namespace Todo.Api.Features.Queries;

public record GetTodosQuery(int Page = 1, int PageSize = 10) : IQuery<GetTodosResult>;

public record GetTodosResult(IEnumerable<TodoTaskDto> Todos, int TotalCount, int Page, int PageSize);

public record GetTodosResponseDto(IEnumerable<TodoTaskDto> Todos);

public record GetTodosPagedResponseDto(
    IEnumerable<TodoTaskDto> Todos, 
    int Page, 
    int PageSize, 
    int TotalCount, 
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

public class GetTodosEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/todos", async (int? page, int? pageSize, ISender mediator) =>
        {
            var effectivePage = Math.Max(page ?? 1, 1);
            var effectivePageSize = Math.Clamp(pageSize ?? 10, 1, 100);
            
            var query = new GetTodosQuery(effectivePage, effectivePageSize);
            var result = await mediator.Send(query);

            var totalPages = (int)Math.Ceiling((double)result.TotalCount / effectivePageSize);

            var response = new GetTodosPagedResponseDto(
                result.Todos,
                effectivePage,
                effectivePageSize,
                result.TotalCount,
                totalPages,
                effectivePage < totalPages,
                effectivePage > 1);

            return Results.Ok(response);
        });
    }
}

public class GetTodosHandler(ITodosRepository todosRepository) : IQueryHandler<GetTodosQuery, GetTodosResult>
{
    public async Task<GetTodosResult> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var (todos, totalCount) = await todosRepository.GetTodosPagedAsync(request.Page, request.PageSize, cancellationToken);

        var todosDto = todos.Adapt<IEnumerable<TodoTaskDto>>();

        return new GetTodosResult(todosDto, totalCount, request.Page, request.PageSize);
    }
}