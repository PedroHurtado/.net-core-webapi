namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class ActivatePlanPricingTierTests : PlanWebApplicationFixture
{
    public ActivatePlanPricingTierTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Activate_WithValidData_Returns200()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: false);

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Activate_WithValidData_ReturnsIsActiveTrue()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: false);

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/activate", null);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: false);

        await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/activate", null);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"/plans/{nonExistentId}/pricing-tiers/Monthly/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WithNonExistingBillingPeriod_Returns404()
    {
        var plan = await CreatePlanAsync();

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WhenAlreadyActive_Returns409()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: true);

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
