using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Todo.Api.Features.Commands;
using Todo.Api.Features.Queries;
using Todo.Api.Models;

namespace Todo.IntegrationTests.Features.Commands;

public class SetTodoCompletionPercentageTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task SetTodoPercentComplete_ShouldSetPercentComplete_WhenValidRequest()
    {
        // Arrange
        var task = new TodoTask();
        await DbContext.AddAsync(task);
        await DbContext.SaveChangesAsync();

        var setCompletionPercentageRequest = new SetTodoCompletionPercentageRequestDto(50);

        // Act
        var setCompletionPercentageResponse =
            await Client.PatchAsJsonAsync($"/api/todos/{task.Id}/completion-percentage",
                setCompletionPercentageRequest);

        // Assert
        using (new AssertionScope())
        {
            setCompletionPercentageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await setCompletionPercentageResponse.Content
                .ReadFromJsonAsync<SetTodoCompletionPercentageResponseDto>();
            result.Should().NotBeNull();
            result!.Todo.CompletionPercentage.Should().Be(50);
        }
    }

    [Fact]
    public async Task SetTodoPercentComplete_ShouldReturnNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var setPercentCompleteRequest = new SetTodoCompletionPercentageRequestDto(50);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/todos/{nonExistentId}/completion-percentage",
            setPercentCompleteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetTodoPercentComplete_ShouldReturnBadRequest_WhenPercentCompleteIsInvalid()
    {
        // Arrange
        var task = new TodoTask();
        await DbContext.AddAsync(task);
        await DbContext.SaveChangesAsync();

        var setPercentCompleteRequest = new SetTodoCompletionPercentageRequestDto(150); // Invalid percent complete

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/todos/{task.Id}/completion-percentage",
            setPercentCompleteRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}