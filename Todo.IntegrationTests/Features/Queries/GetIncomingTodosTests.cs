using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.Time.Testing;
using Todo.Api.Features.Queries;
using Todo.Api.Models;

namespace Todo.IntegrationTests.Features.Queries;

public class GetIncomingTodosTests(TestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly FakeTimeProvider
        _fakeTimeProvider = new(new DateTimeOffset(2024, 8, 15, 10, 30, 0, TimeSpan.Zero));

    [Fact]
    public async Task GetIncomingTodos_ShouldReturnOkWithTodayTodos_WhenPeriodIsToday()
    {
        // Arrange
        var period = IncomingTodosPeriod.Today;
        var today = _fakeTimeProvider.GetUtcNow().Date;
        var expectedTodos = new List<TodoTask>()
        {
            new TodoTask()
            {
                DueDate = new DateTimeOffset(today.Year, today.Month, today.Day, 9, 0, 0, TimeSpan.Zero) // 9 AM today
            },
            new TodoTask()
            {
                DueDate = new DateTimeOffset(today.Year, today.Month, today.Day, 12, 0, 0, TimeSpan.Zero) // 12 PM today
            },
            new TodoTask()
            {
                DueDate = new DateTimeOffset(today.Year, today.Month, today.Day, 18, 0, 0, TimeSpan.Zero) // 6 PM today
            },
        };
        await DbContext.Todos.AddRangeAsync(expectedTodos);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/todos/incoming?period={period}");

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadFromJsonAsync<GetIncomingTodosResponseDto>();
            responseBody.Should().NotBeNull();
            responseBody!.Todos.Should().HaveCount(expectedTodos.Count);
        }
    }

    [Fact]
    public async Task GetIncomingTodos_ShouldReturnOkWithNextDayTodos_WhenPeriodIsNextDay()
    {
        // Arrange
        var period = IncomingTodosPeriod.NextDay;
        var today = _fakeTimeProvider.GetUtcNow().Date;
        var nextDay = today.AddDays(1);
        var expectedTodos = new List<TodoTask>()
        {
            new()
            {
                DueDate = new DateTimeOffset(nextDay.Year, nextDay.Month, nextDay.Day, 9, 0, 0,
                    TimeSpan.Zero) // 9 AM next day
            },
            new()
            {
                DueDate = new DateTimeOffset(nextDay.Year, nextDay.Month, nextDay.Day, 12, 0, 0,
                    TimeSpan.Zero) // 12 PM next day
            },
            new()
            {
                DueDate = new DateTimeOffset(nextDay.Year, nextDay.Month, nextDay.Day, 18, 0, 0,
                    TimeSpan.Zero) // 6 PM next day
            },
        };
        await DbContext.Todos.AddRangeAsync(expectedTodos);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/api/todos/incoming?period={period}");

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseBody = await response.Content.ReadFromJsonAsync<GetIncomingTodosResponseDto>();
            responseBody.Should().NotBeNull();
            responseBody!.Todos.Should().HaveCount(expectedTodos.Count);
        }
    }
}