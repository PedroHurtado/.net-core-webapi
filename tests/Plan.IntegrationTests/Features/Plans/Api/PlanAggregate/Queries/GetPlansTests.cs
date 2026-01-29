namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Queries;

public class GetPlansTests(WebApplicationFactory<Program> factory)
    : PlanWebApplicationFixture(factory)
{
    [Fact]
    public async Task GetPlans_ReturnsOkAndList()
    {
        var created1 = await CreatePlanAsync(name: "Plan Básico Test");
        var created2 = await CreatePlanAsync(name: "Plan Premium Test");

        var response = await Client.GetAsync("/plans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PlanResponse>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Should().Contain(p => p.Id == created1.Id);
        result.Should().Contain(p => p.Id == created2.Id);
    }

    [Fact]
    public async Task GetPlans_WithNoPlans_ReturnsEmptyArray()
    {
        var response = await Client.GetAsync("/plans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PlanResponse>>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPlans_WithIsActiveTrue_ReturnsOnlyActivePlans()
    {
        var activePlan = await CreateCompletePlanAsync(name: "Plan Activo Filter Test");
        await Client.PostAsync($"/plans/{activePlan.Id}/activate", null);

        var inactivePlan = await CreatePlanAsync(name: "Plan Inactivo Filter Test");

        var response = await Client.GetAsync("/plans?isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PlanResponse>>(JsonOptions);
        result.Should().Contain(p => p.Id == activePlan.Id);
        result.Should().NotContain(p => p.Id == inactivePlan.Id);
    }

    [Fact]
    public async Task GetPlans_WithIsActiveFalse_ReturnsOnlyInactivePlans()
    {
        var activePlan = await CreateCompletePlanAsync(name: "Plan Activo Filter Test 2");
        await Client.PostAsync($"/plans/{activePlan.Id}/activate", null);

        var inactivePlan = await CreatePlanAsync(name: "Plan Inactivo Filter Test 2");

        var response = await Client.GetAsync("/plans?isActive=false");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PlanResponse>>(JsonOptions);
        result.Should().NotContain(p => p.Id == activePlan.Id);
        result.Should().Contain(p => p.Id == inactivePlan.Id);
    }
}
