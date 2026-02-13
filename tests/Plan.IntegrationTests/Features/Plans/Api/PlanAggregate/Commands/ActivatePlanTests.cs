namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class ActivatePlanTests : PlanWebApplicationFixture
{
    public ActivatePlanTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Activate_WithCompletePlan_Returns200()
    {
        var plan = await CreateCompletePlanAsync();

        var response = await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Activate_WithCompletePlan_ReturnsIsActiveTrue()
    {
        var plan = await CreateCompletePlanAsync();

        var response = await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        var content = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        content.Should().NotBeNull();
        content!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithCompletePlan_PersistsActiveState()
    {
        var plan = await CreateCompletePlanAsync();

        await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"/plans/{nonExistentId}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WithAlreadyActivePlan_Returns409()
    {
        var plan = await CreateCompletePlanAsync();
        await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        var response = await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Activate_WithPlanWithoutFeatures_Returns422()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: true);
        await AddPricingTierProviderConfigToPlanAsync(plan.Id);

        var response = await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Activate_WithPlanWithoutActivePricingTier_Returns422()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id);

        var response = await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
