using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using NSubstitute;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Commands;
using Todo.Api.Models;

namespace Todo.UnitTests.Features.Commands;

public class SetTodoCompletionPercentageTests
{
    private readonly SetTodoCompletionPercentageHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();

    public SetTodoCompletionPercentageTests()
    {
        _handler = new SetTodoCompletionPercentageHandler(_todosRepository);
    }

    [Fact]
    public async Task Handle_ShouldSetCompletionPercentage_WhenTodoExists()
    {
        // Arrange
        var fixture = new Fixture();
        var existing = fixture.Create<TodoTask>();
        var newPercent = 75;
        _todosRepository.GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(new SetTodoCompletionPercentageCommand(existing.Id, newPercent),
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<SetTodoCompletionPercentageResult>()
            .Which.Todo.CompletionPercentage.Should().Be(newPercent);
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
        var act = async () =>
            await _handler.Handle(new SetTodoCompletionPercentageCommand(id, 50), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TodoTaskNotFoundException>();
        await _todosRepository.Received(1).GetTodoByIdAsync(id, Arg.Any<CancellationToken>());
    }
}