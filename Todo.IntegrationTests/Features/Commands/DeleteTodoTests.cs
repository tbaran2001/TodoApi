using System.Net;
using AwesomeAssertions;
using Todo.Api.Models;

namespace Todo.IntegrationTests.Features.Commands;

public class DeleteTodoTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteTodo_ShouldDeleteTodoTask_WhenValidRequest()
    {
        // Arrange
        var task = new TodoTask();
        await DbContext.AddAsync(task);
        await DbContext.SaveChangesAsync();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/todos/{task.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTodo_ShouldReturnNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/todos/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}