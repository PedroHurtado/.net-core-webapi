namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePricingTierProviderConfigurationTests : PlanWebApplicationFixture
{
    public UpdatePricingTierProviderConfigurationTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Update_WithValidData_Returns200()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);
        await AddPricingTierProviderConfigToPlanAsync(plan.Id);

        var request = new { ExternalProductId = "prod_updated", ExternalPriceId = "price_updated" };

        var response = await Client.PutAsJsonAsync(
            $"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations/Stripe", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedConfig()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);
        await AddPricingTierProviderConfigToPlanAsync(plan.Id);

        var request = new { ExternalProductId = "prod_updated", ExternalPriceId = "price_updated" };

        var response = await Client.PutAsJsonAsync(
            $"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations/Stripe", request);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        var config = tier.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.ExternalProductId.Should().Be("prod_updated");
        config.ExternalPriceId.Should().Be("price_updated");
    }

    [Fact]
    public async Task Update_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);
        await AddPricingTierProviderConfigToPlanAsync(plan.Id);

        var request = new { ExternalProductId = "prod_updated", ExternalPriceId = "price_updated" };
        await Client.PutAsJsonAsync(
            $"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations/Stripe", request);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        var config = tier.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.ExternalProductId.Should().Be("prod_updated");
    }

    [Fact]
    public async Task Update_PreservesIsActiveStatus()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);
        await AddPricingTierProviderConfigToPlanAsync(plan.Id, isActive: true);

        var request = new { ExternalProductId = "prod_updated", ExternalPriceId = "price_updated" };
        var response = await Client.PutAsJsonAsync(
            $"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations/Stripe", request);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        var tier = result!.PricingTiers.First(t => t.BillingPeriod == BillingPeriod.Monthly);
        var config = tier.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Update_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new { ExternalProductId = "prod_updated", ExternalPriceId = "price_updated" };

        var response = await Client.PutAsJsonAsync(
            $"/plans/{nonExistentId}/pricing-tiers/Monthly/provider-configurations/Stripe", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithNonExistingBillingPeriod_Returns404()
    {
        var plan = await CreatePlanAsync();

        var request = new { ExternalProductId = "prod_updated", ExternalPriceId = "price_updated" };

        var response = await Client.PutAsJsonAsync(
            $"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations/Stripe", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithNonExistingProvider_Returns404()
    {
        var plan = await CreatePlanAsync();
        await AddPricingTierToPlanAsync(plan.Id);

        var request = new { ExternalProductId = "prod_updated", ExternalPriceId = "price_updated" };

        var response = await Client.PutAsJsonAsync(
            $"/plans/{plan.Id}/pricing-tiers/Monthly/provider-configurations/NonExistent", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
