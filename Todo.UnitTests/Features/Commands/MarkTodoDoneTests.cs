using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Commands;
using Todo.Api.Models;

namespace Todo.UnitTests.Features.Commands;

public class MarkTodoDoneTests
{
    private readonly MarkTodoDoneHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();

    public MarkTodoDoneTests()
    {
        _handler = new MarkTodoDoneHandler(_todosRepository);
    }

    [Fact]
    public async Task Handle_ShouldMarkTodoDone_WhenTodoExists()
    {
        // Arrange
        var fixture = new Fixture();
        var existing = fixture.Create<TodoTask>();
        _todosRepository.GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(new MarkTodoDoneCommand(existing.Id), CancellationToken.None);

        // Assert
        result.Should().BeOfType<MarkTodoDoneResult>()
            .Which.Todo.CompletionPercentage.Should().Be(100);
        existing.CompletionPercentage.Should().Be(100);
        await _todosRepository.Received(1).GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowTodoTaskNotFoundException_WhenTodoDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _todosRepository.GetTodoByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((TodoTask?)null);

        // Act
        var act = async () => await _handler.Handle(new MarkTodoDoneCommand(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TodoTaskNotFoundException>();
        await _todosRepository.Received(1).GetTodoByIdAsync(id, Arg.Any<CancellationToken>());
    }
}