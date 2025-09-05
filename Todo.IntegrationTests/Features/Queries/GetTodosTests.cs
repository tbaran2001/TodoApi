using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Todo.Api.Features.Queries;
using Todo.Api.Models;

namespace Todo.IntegrationTests.Features.Queries;

public class GetTodosTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetTodos_ShouldReturnTodos_WhenTodosExist()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask(),
            new TodoTask()
        };
        await DbContext.Todos.AddRangeAsync(tasks);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/api/todos");

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTodosResponseDto>();
            result.Should().NotBeNull();
            result.Todos.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetTodos_ShouldReturnEmptyList_WhenNoTodosExist()
    {
        // Arrange
        
        // Act
        var response = await Client.GetAsync("/api/todos");

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<GetTodosResponseDto>();
            result.Should().NotBeNull();
            result.Todos.Should().BeEmpty();
        }
    }
}