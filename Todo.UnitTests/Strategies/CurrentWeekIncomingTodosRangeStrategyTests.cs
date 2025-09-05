using AwesomeAssertions;
using Todo.Api.Features.Queries;
using Todo.Api.Strategies;

namespace Todo.UnitTests.Strategies;

public class CurrentWeekIncomingTodosRangeStrategyTests
{
    private readonly CurrentWeekIncomingTodosRangeStrategy _strategy = new();

    public static IEnumerable<object[]> GetRangeCases =>
    [
        // Sunday -> week is Monday-Sunday per strategy
        [
            new DateTimeOffset(2023, 10, 01, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 09, 25, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 01, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        // Monday
        [
            new DateTimeOffset(2023, 10, 02, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 08, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        // Tuesday
        [
            new DateTimeOffset(2023, 10, 03, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 08, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        // Wednesday
        [
            new DateTimeOffset(2023, 10, 04, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 08, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        // Thursday
        [
            new DateTimeOffset(2023, 10, 05, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 08, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        // Friday
        [
            new DateTimeOffset(2023, 10, 06, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 08, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
        ],
        // Saturday
        [
            new DateTimeOffset(2023, 10, 07, 10, 15, 30, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 02, 00, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 08, 23, 59, 59, TimeSpan.Zero).AddTicks(9_999_999)
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
    public void Period_ShouldReturnCurrentWeek()
    {
        // Act
        var period = _strategy.Period;

        // Assert
        period.Should().Be(IncomingTodosPeriod.CurrentWeek);
    }
}