using AutoFixture;
using AwesomeAssertions;
using MediatR;
using NSubstitute;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Commands;
using Todo.Api.Models;

namespace Todo.UnitTests.Features.Commands;

public class DeleteTodoTests
{
    private readonly DeleteTodoHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();

    public DeleteTodoTests()
    {
        _handler = new DeleteTodoHandler(_todosRepository);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTodoTask_WhenTodoExists()
    {
        // Arrange
        var fixture = new Fixture();
        var existing = fixture.Create<TodoTask>();
        _todosRepository.GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(new DeleteTodoCommand(existing.Id), CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        await _todosRepository.Received(1)
            .GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>());
        _todosRepository.Received(1)
            .RemoveTodoAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowTodoTaskNotFoundException_WhenTodoDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _todosRepository.GetTodoByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((TodoTask?)null);

        // Act
        var act = async () => await _handler.Handle(new DeleteTodoCommand(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TodoTaskNotFoundException>();
    }
}