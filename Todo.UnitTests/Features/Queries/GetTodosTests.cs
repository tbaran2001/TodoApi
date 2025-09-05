using AutoFixture;
using AwesomeAssertions;
using NSubstitute;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Queries;
using Todo.Api.Models;

namespace Todo.UnitTests.Features.Queries;

public class GetTodosTests
{
    private readonly GetTodosHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();

    public GetTodosTests()
    {
        _handler = new GetTodosHandler(_todosRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTodos_WhenExists()
    {
        // Arrange
        var fixture = new Fixture();
        var todos = fixture.CreateMany<TodoTask>(2).ToArray();

        _todosRepository.GetAllTodosAsync(Arg.Any<CancellationToken>())
            .Returns(todos);

        // Act
        var result = await _handler.Handle(new GetTodosQuery(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetTodosResult>()
            .Which.Todos.Should().BeEquivalentTo(todos);
        await _todosRepository.Received(1).GetAllTodosAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        // Arrange
        _todosRepository.GetAllTodosAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(new GetTodosQuery(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetTodosResult>()
            .Which.Todos.Should().BeEmpty();
        await _todosRepository.Received(1).GetAllTodosAsync(Arg.Any<CancellationToken>());
    }
}