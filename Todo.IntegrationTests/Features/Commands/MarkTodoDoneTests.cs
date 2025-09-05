using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Todo.Api.Features.Commands;
using Todo.Api.Features.Queries;
using Todo.Api.Models;

namespace Todo.IntegrationTests.Features.Commands;

public class MarkTodoDoneTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task MarkTodoDone_ShouldMarkTodoAsDone_WhenValidRequest()
    {
        // Arrange
        var task = new TodoTask();
        await DbContext.AddAsync(task);
        await DbContext.SaveChangesAsync();

        // Act
        var markDoneResponse = await Client.PatchAsync($"/api/todos/{task.Id}/done", null);

        // Assert
        using (new AssertionScope())
        {
            markDoneResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await markDoneResponse.Content.ReadFromJsonAsync<MarkTodoDoneResponseDto>();
            result.Should().BeOfType<MarkTodoDoneResponseDto>()
                .Which.Todo.CompletionPercentage.Should().Be(100);
        }
    }

    [Fact]
    public async Task MarkTodoDone_ShouldReturnNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PatchAsync($"/api/todos/{nonExistentId}/done", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}