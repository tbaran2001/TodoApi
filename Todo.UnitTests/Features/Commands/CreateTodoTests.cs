using AutoFixture;
using AwesomeAssertions;
using NSubstitute;
using Todo.Api.Data.Repos;
using Todo.Api.Features.Commands;
using Todo.Api.Models;

namespace Todo.UnitTests.Features.Commands;

public class CreateTodoTests
{
    private readonly CreateTodoHandler _handler;
    private readonly ITodosRepository _todosRepository = Substitute.For<ITodosRepository>();

    public CreateTodoTests()
    {
        _handler = new CreateTodoHandler(_todosRepository);
    }

    [Fact]
    public async Task Handle_ShouldCreateTodoTask_WhenValidRequest()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateTodoCommand>();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreateTodoResult>()
            .Which.Todo.Should().NotBeNull();

        await _todosRepository.Received(1).AddTodoAsync(Arg.Any<TodoTask>(), Arg.Any<CancellationToken>());
    }
}