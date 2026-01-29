namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class ActivateProviderConfigurationTests : PlanWebApplicationFixture
{
    public ActivateProviderConfigurationTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Activate_WithValidData_Returns200()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: false);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Activate_WithValidData_ReturnsPlanResponse()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: false);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/activate", null);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(plan.Id);
        var config = result.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: false);

        await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/activate", null);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var config = result!.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"/plans/{nonExistentId}/provider-configurations/Stripe/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WithNonExistingProvider_Returns404()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: false);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/NonExistent/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Activate_WithAnotherActiveForSameProvider_Returns409()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_inactive", "price_inactive", isActive: false);
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_active", "price_active", isActive: true);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
