using AutoFixture;
using AwesomeAssertions;
using NSubstitute;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Queries;
using Todo.Api.Models;

namespace Todo.UnitTests.Features.Queries;

public class GetTodoByIdTests
{
    private readonly GetTodoByIdHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();

    public GetTodoByIdTests()
    {
        _handler = new GetTodoByIdHandler(_todosRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnTodo_WhenExists()
    {
        // Arrange
        var fixture = new Fixture();
        var existing = fixture.Create<TodoTask>();

        _todosRepository.GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(new GetTodoByIdQuery(existing.Id), CancellationToken.None);

        // Assert
        result.Should().BeOfType<GetTodoByIdResult>()
            .Which.Todo.Should().BeEquivalentTo(existing);
        await _todosRepository.Received(1).GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowTodoTaskNotFoundException_WhenNotFound()
    {
        // Arrange
        _todosRepository.GetTodoByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((TodoTask?)null);

        // Act
        var act = async () => await _handler.Handle(new GetTodoByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TodoTaskNotFoundException>();
        await _todosRepository.Received(1).GetTodoByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}