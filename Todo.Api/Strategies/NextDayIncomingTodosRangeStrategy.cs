using Todo.Api.Features.Queries;

namespace Todo.Api.Strategies;

public class NextDayIncomingTodosRangeStrategy : IIncomingTodosRangeStrategy
{
    public IncomingTodosPeriod Period => IncomingTodosPeriod.NextDay;

    public (DateTimeOffset from, DateTimeOffset to) GetRange(DateTimeOffset now)
    {
        var midnight = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var from = midnight.AddDays(1);
        var to = midnight.AddDays(2).AddTicks(-1);
        return (from, to);
    }
}