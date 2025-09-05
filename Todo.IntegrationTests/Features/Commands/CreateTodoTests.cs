using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Features.Commands;

namespace Todo.IntegrationTests.Features.Commands;

public class CreateTodoTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateTodo_ShouldCreateTodoTask_WhenValidRequest()
    {
        // Arrange
        var request = new CreateTodoRequestDto("Test Title", "Test Description", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var response = await Client.PostAsJsonAsync("/api/todos", request);

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<CreateTodoResponseDto>();
            result.Should().BeOfType<CreateTodoResponseDto>()
                .Which.Todo.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateTodo_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var request = new CreateTodoRequestDto("", "", DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        var response = await Client.PostAsJsonAsync("/api/todos", request);

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problemDetails.Should().NotBeNull();
        }
    }
}