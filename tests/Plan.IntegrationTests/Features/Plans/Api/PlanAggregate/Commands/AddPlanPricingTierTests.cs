namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class AddPlanPricingTierTests : PlanWebApplicationFixture
{
    public AddPlanPricingTierTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Add_WithValidData_Returns201()
    {
        var plan = await CreatePlanAsync();
        var request = new { BillingPeriod = "Monthly", Amount = 9.99m, CurrencyCode = "EUR", IsActive = false };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Add_WithValidData_ReturnsPlanWithPricingTier()
    {
        var plan = await CreatePlanAsync();
        var request = new { BillingPeriod = "Monthly", Amount = 9.99m, CurrencyCode = "EUR", IsActive = false };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers", request);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.PricingTiers.Should().ContainSingle(t => t.BillingPeriod == BillingPeriod.Monthly);
        result.PricingTiers.First().Price.Amount.Should().Be(9.99m);
        result.PricingTiers.First().Price.Currency.Code.Should().Be("EUR");
    }

    [Fact]
    public async Task Add_WithValidData_PersistsPricingTier()
    {
        var plan = await CreatePlanAsync();
        var request = new { BillingPeriod = "Monthly", Amount = 9.99m, CurrencyCode = "EUR", IsActive = false };

        await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers", request);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        result!.PricingTiers.Should().ContainSingle(t => t.BillingPeriod == BillingPeriod.Monthly);
    }

    [Fact]
    public async Task Add_WithIsActiveTrue_ReturnsActiveTier()
    {
        var plan = await CreatePlanAsync();
        var request = new { BillingPeriod = "Monthly", Amount = 9.99m, CurrencyCode = "EUR", IsActive = true };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers", request);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result!.PricingTiers.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Add_MultipleBillingPeriods_ReturnsAllTiers()
    {
        var plan = await CreatePlanAsync();

        await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers",
            new { BillingPeriod = "Monthly", Amount = 9.99m, CurrencyCode = "EUR" });

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers",
            new { BillingPeriod = "Yearly", Amount = 99.99m, CurrencyCode = "EUR" });

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result!.PricingTiers.Should().HaveCount(2);
    }

    [Fact]
    public async Task Add_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new { BillingPeriod = "Monthly", Amount = 9.99m, CurrencyCode = "EUR" };

        var response = await Client.PostAsJsonAsync($"/plans/{nonExistentId}/pricing-tiers", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Add_WithDuplicateBillingPeriod_Returns409()
    {
        var plan = await CreatePlanAsync();
        var request = new { BillingPeriod = "Monthly", Amount = 9.99m, CurrencyCode = "EUR" };

        await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers", request);
        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Add_WithNegativeAmount_Returns422()
    {
        var plan = await CreatePlanAsync();
        var request = new { BillingPeriod = "Monthly", Amount = -5m, CurrencyCode = "EUR" };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/pricing-tiers", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
