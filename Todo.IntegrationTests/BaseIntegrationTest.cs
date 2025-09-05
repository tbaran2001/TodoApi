using Microsoft.Extensions.DependencyInjection;
using Todo.Api.Data;

namespace Todo.IntegrationTests;

public class BaseIntegrationTest : IClassFixture<TestWebAppFactory>, IDisposable
{
    protected HttpClient Client { get; }
    private readonly IServiceScope _scope;
    protected ApplicationDbContext DbContext { get; }

    protected BaseIntegrationTest(TestWebAppFactory factory)
    {
        Client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}