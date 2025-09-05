using AwesomeAssertions;
using Todo.Api.Features.Queries;
using Todo.Api.Strategies;

namespace Todo.UnitTests.Strategies;

public class NextDayIncomingTodosRangeStrategyTests
{
    private readonly NextDayIncomingTodosRangeStrategy _strategy = new();

    public static IEnumerable<object[]> GetRangeCases =>
    [
        [
            new DateTimeOffset(2023, 10, 01, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        [
            new DateTimeOffset(2023, 10, 01, 23, 59, 59, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        [
            new DateTimeOffset(2023, 10, 01, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        [
            new DateTimeOffset(2023, 12, 31, 23, 59, 59, TimeSpan.Zero),
            new DateTimeOffset(2024, 01, 01, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2024, 01, 01, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ]
    ];

    [Theory]
    [MemberData(nameof(GetRangeCases))]
    public void GetRange_ShouldReturnCorrectRange(
        DateTimeOffset now,
        DateTimeOffset expectedFrom,
        DateTimeOffset expectedTo
    )
    {
        // Act
        var (from, to) = _strategy.GetRange(now);

        // Assert
        from.Should().Be(expectedFrom);
        to.Should().Be(expectedTo);
    }

    [Fact]
    public void Period_ShouldReturnNextDay()
    {
        // Act
        var period = _strategy.Period;

        // Assert
        period.Should().Be(IncomingTodosPeriod.NextDay);
    }
}