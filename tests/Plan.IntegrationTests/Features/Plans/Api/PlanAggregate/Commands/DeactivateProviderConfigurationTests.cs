namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class DeactivateProviderConfigurationTests : PlanWebApplicationFixture
{
    public DeactivateProviderConfigurationTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Deactivate_WithValidData_Returns200()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: true);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Deactivate_WithValidData_ReturnsPlanResponse()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: true);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/deactivate", null);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(plan.Id);
        var config = result.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: true);

        await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/deactivate", null);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var config = result!.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"/plans/{nonExistentId}/provider-configurations/Stripe/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WithNonExistingProvider_Returns404()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: true);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/NonExistent/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenOnlyActiveConfigInActivePlan_Returns422()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id);
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "prod_123", "price_123", isActive: true);
        await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        var response = await Client.PostAsync($"/plans/{plan.Id}/provider-configurations/Stripe/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
