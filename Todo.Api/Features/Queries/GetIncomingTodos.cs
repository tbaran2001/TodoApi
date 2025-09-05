using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Todo.Api.Common.Cqrs;
using Todo.Api.Data.Repos;
using Todo.Api.Dtos;
using Todo.Api.Providers;
using Todo.Api.Strategies;

namespace Todo.Api.Features.Queries;

public enum IncomingTodosPeriod
{
    Today,
    NextDay,
    CurrentWeek
}

public record GetIncomingTodosQuery(IncomingTodosPeriod Period) : IQuery<GetIncomingTodosResult>;

public record GetIncomingTodosResult(IEnumerable<TodoTaskDto> Todos);

public record GetIncomingTodosResponseDto(IEnumerable<TodoTaskDto> Todos);

public class GetIncomingTodosEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/todos/incoming",
            async (IncomingTodosPeriod? period, ISender mediator) =>
            {
                var effectivePeriod = period ?? IncomingTodosPeriod.CurrentWeek;

                var query = new GetIncomingTodosQuery(effectivePeriod);

                var result = await mediator.Send(query);

                var response = result.Adapt<GetIncomingTodosResponseDto>();

                return Results.Ok(response);
            });
    }
}

public class GetIncomingTodosHandler(ITodosRepository todosRepository, IIncomingTodosRangeProvider rangeProvider)
    : IQueryHandler<GetIncomingTodosQuery, GetIncomingTodosResult>
{
    public async Task<GetIncomingTodosResult> Handle(GetIncomingTodosQuery request, CancellationToken cancellationToken)
    {
        var (from, to) = rangeProvider.GetRange(request.Period);

        var todos = await todosRepository.GetIncomingTodosAsync(from, to, cancellationToken);

        var todosDto = todos.Adapt<IEnumerable<TodoTaskDto>>();

        return new GetIncomingTodosResult(todosDto);
    }
}

public class GetIncomingTodosValidator : AbstractValidator<GetIncomingTodosQuery>
{
    public GetIncomingTodosValidator()
    {
        RuleFor(x => x.Period)
            .IsInEnum();
    }
}