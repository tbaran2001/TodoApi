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
    
}