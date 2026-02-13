namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class DeactivatePlanPricingTierTests : PlanWebApplicationFixture
{
    public DeactivatePlanPricingTierTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Deactivate_WithValidData_Returns200()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: true);

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Deactivate_WithValidData_ReturnsIsActiveFalse()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: true);

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/deactivate", null);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: true);

        await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/deactivate", null);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"/plans/{nonExistentId}/pricing-tiers/Monthly/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WithNonExistingBillingPeriod_Returns404()
    {
        var plan = await CreatePlanAsync();

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_LastActiveTierOfActivePlan_Returns422()
    {
        var plan = await CreateCompletePlanAsync();
        await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        var response = await Client.PostAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
