using Todo.Api.Features.Queries;

namespace Todo.Api.Strategies;

public class TodayIncomingTodosRangeStrategy : IIncomingTodosRangeStrategy
{
    public IncomingTodosPeriod Period => IncomingTodosPeriod.Today;

    public (DateTimeOffset from, DateTimeOffset to) GetRange(DateTimeOffset now)
    {
        var midnight = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var from = midnight;
        var to = midnight.AddDays(1).AddTicks(-1);
        return (from, to);
    }
}