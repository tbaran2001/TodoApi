using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using NSubstitute;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Queries;
using Todo.Api.Models;
using Todo.Api.Providers;

namespace Todo.UnitTests.Features.Queries;

public class GetIncomingTodosTests
{
    private readonly GetIncomingTodosHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();
    private readonly IIncomingTodosRangeProvider _rangeProvider = Substitute.For<IIncomingTodosRangeProvider>();

    public GetIncomingTodosTests()
    {
        _handler = new GetIncomingTodosHandler(_todosRepository, _rangeProvider);
    }

    [Theory]
    [InlineData(IncomingTodosPeriod.Today)]
    [InlineData(IncomingTodosPeriod.NextDay)]
    [InlineData(IncomingTodosPeriod.CurrentWeek)]
    public async Task Handle_ShouldReturnTodos_ForGivenPeriod(IncomingTodosPeriod period)
    {
        // Arrange
        var from = DateTimeOffset.UtcNow.Date;
        var to = from.AddDays(3);

        var fixture = new Fixture();
        var todos = fixture.CreateMany<TodoTask>(2).ToArray();

        _rangeProvider.GetRange(period).Returns((from, to));
        _todosRepository.GetIncomingTodosAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(todos);

        // Act
        var result = await _handler.Handle(new GetIncomingTodosQuery(period), CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetIncomingTodosResult>()
            .Which.Todos.Should().BeEquivalentTo(todos);

        _rangeProvider.Received(1).GetRange(period);
        await _todosRepository.Received(1)
            .GetIncomingTodosAsync(from, to, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        // Arrange
        var period = IncomingTodosPeriod.Today;
        var from = DateTimeOffset.UtcNow.Date;
        var to = from.AddDays(1);

        _rangeProvider.GetRange(period).Returns((from, to));
        _todosRepository.GetIncomingTodosAsync(from, to, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(new GetIncomingTodosQuery(period), CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetIncomingTodosResult>()
            .Which.Todos.Should().BeEmpty();

        _rangeProvider.Received(1).GetRange(period);
        await _todosRepository.Received(1)
            .GetIncomingTodosAsync(from, to, Arg.Any<CancellationToken>());
    }
}