using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using NSubstitute;
using Todo.Api.Common.Exceptions;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Commands;
using Todo.Api.Models;

namespace Todo.UnitTests.Features.Commands;

public class UpdateTodoTests
{
    private readonly UpdateTodoHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();

    public UpdateTodoTests()
    {
        _handler = new UpdateTodoHandler(_todosRepository);
    }

    [Fact]
    public async Task Handle_ShouldUpdateTodoTask_WhenTodoExists()
    {
        // Arrange
        var fixture = new Fixture();
        var existing = fixture.Create<TodoTask>();
        _todosRepository.GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        var newTitle = "New Title";
        var newDesc = "New Desc";
        var newDue = DateTimeOffset.UtcNow.AddDays(5);
        var command = new UpdateTodoCommand(existing.Id, newTitle, newDesc, newDue);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Todo.Id.Should().Be(existing.Id);
            result.Todo.Title.Should().Be(newTitle);
            result.Todo.Description.Should().Be(newDesc);

            existing.Title.Should().Be(newTitle);
            existing.Description.Should().Be(newDesc);
        }

        await _todosRepository.Received(1).GetTodoByIdAsync(existing.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowTodoTaskNotFoundException_WhenTodoDoesNotExist()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<UpdateTodoCommand>();
        _todosRepository.GetTodoByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((TodoTask?)null);
        
        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<TodoTaskNotFoundException>();
        await _todosRepository.Received(1).GetTodoByIdAsync(command.Id, Arg.Any<CancellationToken>());
    }
}