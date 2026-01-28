namespace Plans.IntegrationTests.Features.Plans.Api.Commands.Plan;

public class UpdateProviderConfigurationTests : PlanWebApplicationFixture
{
    public UpdateProviderConfigurationTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task UpdateProviderConfig_WithValidData_Returns204()
    {
        // Arrange
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "old_prod", "old_price");

        var request = new UpdateProviderConfiguration.Request("new_prod", "new_price");

        // Act
        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/provider-configurations/Stripe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProviderConfig_Persistence_Check()
    {
        // Arrange
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "old_prod", "old_price");

        var request = new UpdateProviderConfiguration.Request("new_prod", "new_price");

        // Act
        var putResponse = await Client.PutAsJsonAsync($"/plans/{plan.Id}/provider-configurations/Stripe", request);
        putResponse.EnsureSuccessStatusCode();

        // Assert
        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var updatedPlan = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        updatedPlan!.ProviderConfigurations.Should().ContainSingle(c => 
            c.Provider == "Stripe" && 
            c.ExternalProductId == "new_prod" && 
            c.ExternalPriceId == "new_price");
    }

    [Fact]
    public async Task UpdateProviderConfig_WithNonExistingId_Returns404()
    {
        // Arrange
        var request = new UpdateProviderConfiguration.Request("new_prod", "new_price");

        // Act
        var response = await Client.PutAsJsonAsync($"/plans/{Guid.NewGuid()}/provider-configurations/Stripe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProviderConfig_WithInvalidData_Returns422()
    {
        // Arrange
        var plan = await CreatePlanAsync();
        await AddProviderConfigToPlanAsync(plan.Id, "Stripe", "old_prod", "old_price");

        var request = new UpdateProviderConfiguration.Request("", ""); // Invalid

        // Act
        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/provider-configurations/Stripe", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
