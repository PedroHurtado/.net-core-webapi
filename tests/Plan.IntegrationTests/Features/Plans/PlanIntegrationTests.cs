namespace Plan.IntegrationTests.Features.Plans;

public class PlanIntegrationTests(WebApplicationFactory<Program> factory)
    : PlanWebApplicationFixture(factory)
{
    [Fact]
    public void Application_ShouldStart_And_ResolveClient()
    {
        Client.Should().NotBeNull();
    }

    [Fact]
    public void DbContext_ShouldBeResolvable()
    {
        var dbContext = GetService<PlanDbContext>();
        dbContext.Should().NotBeNull();
    }
}
