using AwesomeAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Todo.Api.Common.Behaviors;
using Todo.Api.Common.Cqrs;
using Todo.Api.Data;

namespace Todo.UnitTests.Behaviors;

public class DummyCommand : ICommand<DummyResponse>;

public class SaveChangesBehaviorTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RequestHandlerDelegate<DummyResponse> _next;
    private readonly SaveChangesBehavior<DummyCommand, DummyResponse> _behavior;

    public SaveChangesBehaviorTests()
    {
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        _dbContext = Substitute.For<ApplicationDbContext>(dbContextOptions);

        _next = Substitute.For<RequestHandlerDelegate<DummyResponse>>();

        _behavior = new SaveChangesBehavior<DummyCommand, DummyResponse>(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_ThenSaveChanges_AndReturnResponse()
    {
        // Arrange
        var command = new DummyCommand();
        var expectedResponse = new DummyResponse();

        _next(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _behavior.Handle(command, _next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        await _next.Received(1).Invoke(Arg.Any<CancellationToken>());
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_ShouldNotCallSaveChanges()
    {
        // Arrange
        var command = new DummyCommand();
        var testException = new InvalidOperationException("Handler failed");

        _next(Arg.Any<CancellationToken>()).ThrowsAsync(testException);

        // Act
        Func<Task> act = async () => await _behavior.Handle(command, _next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(testException.Message);
        await _dbContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}