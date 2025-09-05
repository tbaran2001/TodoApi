using Todo.Api.Features.Queries;

namespace Todo.Api.Strategies;

public interface IIncomingTodosRangeStrategy
{
    IncomingTodosPeriod Period { get; }
    (DateTimeOffset from, DateTimeOffset to) GetRange(DateTimeOffset now);
}