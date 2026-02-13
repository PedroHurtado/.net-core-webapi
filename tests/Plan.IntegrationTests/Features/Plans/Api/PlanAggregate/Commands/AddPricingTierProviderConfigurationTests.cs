namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class AddPricingTierProviderConfigurationTests : PlanWebApplicationFixture
{
    public AddPricingTierProviderConfigurationTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Add_WithValidData_Returns201()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { Provider = "Stripe", ExternalProductId = "prod_123", ExternalPriceId = "price_123", IsActive = true };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Add_WithValidData_ReturnsProviderConfig()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { Provider = "Stripe", ExternalProductId = "prod_123", ExternalPriceId = "price_123", IsActive = true };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations", request);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.ProviderConfigurations.Should().ContainSingle(c => c.Provider == "Stripe");
    }

    [Fact]
    public async Task Add_WithValidData_PersistsConfig()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { Provider = "Stripe", ExternalProductId = "prod_123", ExternalPriceId = "price_123", IsActive = true };
        await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations", request);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.ProviderConfigurations.Should().ContainSingle(c => c.Provider == "Stripe");
    }

    [Fact]
    public async Task Add_MultipleProviders_ReturnsAll()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations",
            new { Provider = "Stripe", ExternalProductId = "prod_1", ExternalPriceId = "price_1", IsActive = true });

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations",
            new { Provider = "Paddle", ExternalProductId = "prod_2", ExternalPriceId = "price_2", IsActive = true });

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        tier.ProviderConfigurations.Should().HaveCount(2);
    }

    [Fact]
    public async Task Add_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new { Provider = "Stripe", ExternalProductId = "prod_123", ExternalPriceId = "price_123" };

        var response = await Client.PostAsJsonAsync($"/plans/{nonExistentId}/pricing-tiers/Monthly/provider-configurations", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Add_WithNonExistingBillingPeriod_Returns404()
    {
        var plan = await CreatePlanAsync();

        var request = new { Provider = "Stripe", ExternalProductId = "prod_123", ExternalPriceId = "price_123" };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Add_WithDuplicateProvider_Returns409()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { Provider = "Stripe", ExternalProductId = "prod_123", ExternalPriceId = "price_123" };

        await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations", request);
        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
