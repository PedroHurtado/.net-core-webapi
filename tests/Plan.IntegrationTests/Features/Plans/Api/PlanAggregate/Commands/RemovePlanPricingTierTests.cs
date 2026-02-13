namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class RemovePlanPricingTierTests : PlanWebApplicationFixture
{
    public RemovePlanPricingTierTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Remove_WithValidData_Returns204()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var response = await Client.DeleteAsync($"/plans/{plan.Id}/pricing-tiers/Monthly");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Remove_WithValidData_PersistsRemoval()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        await Client.DeleteAsync($"/plans/{plan.Id}/pricing-tiers/Monthly");

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        result!.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public async Task Remove_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"/plans/{nonExistentId}/pricing-tiers/Monthly");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remove_WithNonExistingBillingPeriod_Returns404()
    {
        var plan = await CreatePlanAsync();

        var response = await Client.DeleteAsync($"/plans/{plan.Id}/pricing-tiers/Monthly");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remove_LastActiveTierFromActivePlan_Returns422()
    {
        var plan = await CreateCompletePlanAsync();
        await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        var response = await Client.DeleteAsync($"/plans/{plan.Id}/pricing-tiers/Monthly");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
