namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlanPricingTierTests : PlanWebApplicationFixture
{
    public UpdatePlanPricingTierTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Update_WithValidData_Returns200()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { Amount = 19.99m, CurrencyCode = "USD" };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedPrice()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { Amount = 19.99m, CurrencyCode = "USD" };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly", request);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.Price.Amount.Should().Be(19.99m);
        tier.Price.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public async Task Update_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { Amount = 19.99m, CurrencyCode = "USD" };
        await Client.PutAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly", request);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.Price.Amount.Should().Be(19.99m);
    }

    [Fact]
    public async Task Update_PreservesIsActiveStatus()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id, isActive: true);

        var request = new { Amount = 19.99m, CurrencyCode = "USD" };
        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly", request);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Update_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new { Amount = 19.99m, CurrencyCode = "USD" };

        var response = await Client.PutAsJsonAsync($"/plans/{nonExistentId}/pricing-tiers/Monthly", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithNonExistingBillingPeriod_Returns404()
    {
        var plan = await CreatePlanAsync();

        var request = new { Amount = 19.99m, CurrencyCode = "USD" };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
