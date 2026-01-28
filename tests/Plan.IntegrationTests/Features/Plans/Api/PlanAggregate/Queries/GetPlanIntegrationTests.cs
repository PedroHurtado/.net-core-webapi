namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Queries;

public class GetPlanIntegrationTests : PlanWebApplicationFixture
{
    public GetPlanIntegrationTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Get_WithExistingId_Returns200()
    {
        var created = await CreatePlanAsync(name: "Plan Básico");

        var response = await Client.GetAsync($"/plans/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be("Plan Básico");
    }

    [Fact]
    public async Task Get_WithNonExistingId_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"/plans/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
