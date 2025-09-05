using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Todo.Api.Features.Commands;
using Todo.Api.Models;

namespace Todo.IntegrationTests.Features.Commands;

public class UpdateTodoTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateTodo_ShouldUpdateTodo_WhenValidRequest()
    {
        // Arrange
        var task = new TodoTask();
        await DbContext.AddAsync(task);
        await DbContext.SaveChangesAsync();

        var updateRequest =
            new UpdateTodoRequestDto("Updated Title", "Updated Description", DateTimeOffset.UtcNow.AddDays(2));

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"/api/todos/{task.Id}", updateRequest);

        // Assert
        using (new AssertionScope())
        {
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await updateResponse.Content.ReadFromJsonAsync<UpdateTodoResponseDto>();
            result.Should().BeOfType<UpdateTodoResponseDto>()
                .Which.Todo.Should().NotBeNull();
            result.Todo.Title.Should().Be(updateRequest.Title);
            result.Todo.Description.Should().Be(updateRequest.Description);
            result.Todo.DueDate.Should().Be(updateRequest.DueDate);
        }
    }

    [Fact]
    public async Task UpdateTodo_ShouldReturnNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest =
            new UpdateTodoRequestDto("Updated Title", "Updated Description", DateTimeOffset.UtcNow.AddDays(2));

        // Act
        var response = await Client.PutAsJsonAsync($"/api/todos/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTodo_ShouldReturnBadRequest_WhenDueDateIsInThePast()
    {
        // Arrange
        var task = new TodoTask()
        {
            DueDate = DateTimeOffset.UtcNow.AddDays(5)
        };
        await DbContext.AddAsync(task);
        await DbContext.SaveChangesAsync();

        var pastDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var updateRequest =
            new UpdateTodoRequestDto("Updated Title", "Updated Description", pastDate);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/todos/{task.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}