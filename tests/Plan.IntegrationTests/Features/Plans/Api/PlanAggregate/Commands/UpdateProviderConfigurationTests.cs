namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdateProviderConfigurationTests : PlanWebApplicationFixture
{
    public UpdateProviderConfigurationTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Update_WithValidData_Returns204()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "old_prod", "old_price");
        var request = new { ExternalProductId = "new_prod", ExternalPriceId = "new_price" };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/provider-configurations/Stripe", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "old_prod", "old_price");
        var request = new { ExternalProductId = "new_prod", ExternalPriceId = "new_price" };

        await Client.PutAsJsonAsync($"/plans/{plan.Id}/provider-configurations/Stripe", request);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var config = result!.ProviderConfigurations.First(c => c.Provider == "Stripe");
        config.ExternalProductId.Should().Be("new_prod");
        config.ExternalPriceId.Should().Be("new_price");
    }

    [Fact]
    public async Task Update_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new { ExternalProductId = "new_prod", ExternalPriceId = "new_price" };

        var response = await Client.PutAsJsonAsync($"/plans/{nonExistentId}/provider-configurations/Stripe", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithNonExistingProvider_Returns404()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "old_prod", "old_price");
        var request = new { ExternalProductId = "new_prod", ExternalPriceId = "new_price" };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/provider-configurations/NonExistent", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEmptyProductId_Returns422()
    {
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "old_prod", "old_price");
        var request = new { ExternalProductId = "", ExternalPriceId = "" };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/provider-configurations/Stripe", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
