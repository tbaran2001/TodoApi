using AwesomeAssertions;
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Plugins.Http.CSharp;

namespace Todo.PerformaceTests;

public class TodoApiPerformanceTests
{
    [Fact]
    public void GetTodos_ShouldHandleHighLoad()
    {
        const string baseUrl = "http://localhost:5000/api/todos";

        var getTodosStep = Step.Create("Get_Todos", clientFactory: HttpClientFactory.Create(), async context =>
        {
            try
            {
                var request = Http.CreateRequest("GET", baseUrl);
                return await Http.Send(request, context);
            }
            catch
            {
                return Response.Fail();
            }
        });

        const int expectedRequestsPerSecond = 100;
        const int durationInSeconds = 10;
        

        var scenario = ScenarioBuilder.CreateScenario("Get_Todos_Scenario", getTodosStep)
            .WithWarmUpDuration(TimeSpan.FromSeconds(5))
            .WithLoadSimulations(
                Simulation.KeepConstant(expectedRequestsPerSecond, TimeSpan.FromSeconds(durationInSeconds))
            );
        
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
        // Assert
        stats.OkCount.Should().BeGreaterThan((int)(expectedRequestsPerSecond * durationInSeconds * 0.9));
    }

    [Fact]
    public void GetTodosPaginated_ShouldHandleHighLoad()
    {
        const string baseUrl = "http://localhost:5000/api/todos";

        var getTodosPaginatedStep = Step.Create("Get_Todos_Paginated", clientFactory: HttpClientFactory.Create(), async context =>
        {
            try
            {
                var page = Random.Shared.Next(1, 11); // Test different pages
                var pageSize = Random.Shared.Next(5, 21); // Test different page sizes
                var request = Http.CreateRequest("GET", $"{baseUrl}?page={page}&pageSize={pageSize}");
                return await Http.Send(request, context);
            }
            catch
            {
                return Response.Fail();
            }
        });

        const int expectedRequestsPerSecond = 150; // Higher expected performance with pagination
        const int durationInSeconds = 10;

        var scenario = ScenarioBuilder.CreateScenario("Get_Todos_Paginated_Scenario", getTodosPaginatedStep)
            .WithWarmUpDuration(TimeSpan.FromSeconds(5))
            .WithLoadSimulations(
                Simulation.KeepConstant(expectedRequestsPerSecond, TimeSpan.FromSeconds(durationInSeconds))
            );
        
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
        // Assert
        stats.OkCount.Should().BeGreaterThan((int)(expectedRequestsPerSecond * durationInSeconds * 0.9));
    }

    [Fact]
    public void GetIncomingTodos_ShouldHandleHighLoad()
    {
        const string baseUrl = "http://localhost:5000/api/todos/incoming";

        var getIncomingTodosStep = Step.Create("Get_Incoming_Todos", clientFactory: HttpClientFactory.Create(), async context =>
        {
            try
            {
                var periods = new[] { "Today", "NextDay", "CurrentWeek" };
                var period = periods[Random.Shared.Next(periods.Length)];
                var request = Http.CreateRequest("GET", $"{baseUrl}?period={period}");
                return await Http.Send(request, context);
            }
            catch
            {
                return Response.Fail();
            }
        });

        const int expectedRequestsPerSecond = 200; // Higher expected performance with indexes
        const int durationInSeconds = 10;

        var scenario = ScenarioBuilder.CreateScenario("Get_Incoming_Todos_Scenario", getIncomingTodosStep)
            .WithWarmUpDuration(TimeSpan.FromSeconds(5))
            .WithLoadSimulations(
                Simulation.KeepConstant(expectedRequestsPerSecond, TimeSpan.FromSeconds(durationInSeconds))
            );
        
        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
        // Assert - Should handle higher load with database indexes
        stats.OkCount.Should().BeGreaterThan((int)(expectedRequestsPerSecond * durationInSeconds * 0.9));
    }
}