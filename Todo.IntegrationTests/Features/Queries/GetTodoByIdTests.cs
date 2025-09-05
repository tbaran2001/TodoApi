using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Todo.Api.Features.Queries;
using Todo.Api.Models;

namespace Todo.IntegrationTests.Features.Queries;

public class GetTodoByIdTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetTodoById_ShouldReturnTodo_WhenTodoExists()
    {
        // Arrange
        var task = new TodoTask();
        await DbContext.AddAsync(task);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/todos/{task.Id}");

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todo = await response.Content.ReadFromJsonAsync<GetTodoByIdResponseDto>();
            todo.Should().NotBeNull();
            todo.Todo.Id.Should().Be(task.Id);
        }
    }

    [Fact]
    public async Task GetTodoById_ShouldReturnNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/todos/{nonExistentId}");

        // Assert

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}