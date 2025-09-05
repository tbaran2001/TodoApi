using Todo.Api.Features.Queries;

namespace Todo.Api.Strategies;

public class CurrentWeekIncomingTodosRangeStrategy : IIncomingTodosRangeStrategy
{
    public IncomingTodosPeriod Period => IncomingTodosPeriod.CurrentWeek;

    public (DateTimeOffset from, DateTimeOffset to) GetRange(DateTimeOffset now)
    {
        int startOfWeekOffset = now.DayOfWeek == DayOfWeek.Sunday ? -6 : 1 - (int)now.DayOfWeek;

        var midnight = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset);
        var from = midnight.AddDays(startOfWeekOffset);
        var to = from.AddDays(7).AddTicks(-1);

        return (from, to);
    }
}