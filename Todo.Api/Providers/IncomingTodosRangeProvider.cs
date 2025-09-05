using Todo.Api.Features.Queries;
using Todo.Api.Strategies;

namespace Todo.Api.Providers;

public class IncomingTodosRangeProvider(IEnumerable<IIncomingTodosRangeStrategy> strategies, TimeProvider timeProvider)
    : IIncomingTodosRangeProvider
{
    private readonly Dictionary<IncomingTodosPeriod, IIncomingTodosRangeStrategy> _map =
        strategies.ToDictionary(s => s.Period, s => s);

    public (DateTimeOffset from, DateTimeOffset to) GetRange(IncomingTodosPeriod period)
    {
        var now = timeProvider.GetUtcNow();
        if (!_map.TryGetValue(period, out var strategy))
            throw new InvalidOperationException($"No strategy registered for period '{period}'.");
        return strategy.GetRange(now);
    }
}