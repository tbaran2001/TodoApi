using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Todo.Api.Common.Behaviors;

namespace Todo.UnitTests.Behaviors;

public class DummyRequest : IRequest<DummyResponse>;

public class DummyResponse;

public class ValidationBehaviorTests
{
    private readonly IValidator<DummyRequest> _validator;
    private readonly RequestHandlerDelegate<DummyResponse> _next;
    private readonly ValidationBehavior<DummyRequest, DummyResponse> _behavior;

    public ValidationBehaviorTests()
    {
        _validator = Substitute.For<IValidator<DummyRequest>>();
        IEnumerable<IValidator<DummyRequest>> validators = [_validator];
        _next = Substitute.For<RequestHandlerDelegate<DummyResponse>>();
        _behavior = new ValidationBehavior<DummyRequest, DummyResponse>(validators);
    }

    [Fact]
    public async Task Handle_ShouldCallNextDelegateAndReturnResponse_WhenValidatorsWithNoErrors()
    {
        // Arrange
        var request = new DummyRequest();
        var expectedResponse = new DummyResponse();

        _validator.ValidateAsync(Arg.Any<ValidationContext<DummyRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _next(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _behavior.Handle(request, _next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        await _next.Received(1).Invoke(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationExceptionAndNotCallNext_WhenValidatorWithErrors()
    {
        // Arrange
        var request = new DummyRequest();
        var validationFailure = new ValidationFailure("Name", "Name cannot be empty.");
        var validationResult = new ValidationResult([validationFailure]);

        _validator.ValidateAsync(Arg.Any<ValidationContext<DummyRequest>>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        // Act
        var act = async () => await _behavior.Handle(request, _next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 1);
        await _next.DidNotReceive().Invoke(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAggregateAllErrorsAndThrow_WhenMultipleValidatorsWithErrors()
    {
        // Arrange
        var request = new DummyRequest();
        var failure1 = new ValidationFailure("Id", "Id must be positive.");
        var failure2 = new ValidationFailure("Name", "Name is required.");

        var validator1 = Substitute.For<IValidator<DummyRequest>>();
        validator1.ValidateAsync(Arg.Any<ValidationContext<DummyRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([failure1]));

        var validator2 = Substitute.For<IValidator<DummyRequest>>();
        validator2.ValidateAsync(Arg.Any<ValidationContext<DummyRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([failure2]));

        var multipleValidators = new[] { validator1, validator2 };
        var behaviorWithMultipleValidators = new ValidationBehavior<DummyRequest, DummyResponse>(multipleValidators);

        // Act
        var act = async () => await behaviorWithMultipleValidators.Handle(request, _next, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().BeEquivalentTo([failure1, failure2]);
        await _next.DidNotReceive().Invoke(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallNextDelegate_WhenNoValidators()
    {
        // Arrange
        var request = new DummyRequest();
        var expectedResponse = new DummyResponse();

        var behaviorWithNoValidators = new ValidationBehavior<DummyRequest, DummyResponse>(
            []);

        _next(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await behaviorWithNoValidators.Handle(request, _next, CancellationToken.None);

        // Assert
        await _next.Received(1).Invoke(Arg.Any<CancellationToken>());
        result.Should().Be(expectedResponse);
    }
}