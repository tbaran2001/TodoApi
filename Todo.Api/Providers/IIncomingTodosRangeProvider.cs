using Todo.Api.Features.Queries;

namespace Todo.Api.Providers;

public interface IIncomingTodosRangeProvider
{
    (DateTimeOffset from, DateTimeOffset to) GetRange(IncomingTodosPeriod period);
}