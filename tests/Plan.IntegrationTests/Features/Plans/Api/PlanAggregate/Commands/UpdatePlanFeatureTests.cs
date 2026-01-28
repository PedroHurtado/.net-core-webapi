namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class UpdatePlanFeatureTests : PlanWebApplicationFixture
{
    public UpdatePlanFeatureTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Update_WithValidData_Returns204()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id, code: "FEATURE_01", name: "Original Feature");
        var request = new
        {
            Name = "Feature Actualizada",
            Description = "Nueva descripción",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/features/FEATURE_01", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_WithValidData_PersistsChanges()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id, code: "FEATURE_02", name: "Original Feature");
        var request = new
        {
            Name = "Feature Actualizada",
            Description = "Nueva descripción",
            Type = "Limit",
            Limit = 100,
            Unit = "items"
        };

        await Client.PutAsJsonAsync($"/plans/{plan.Id}/features/FEATURE_02", request);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        var feature = result!.Features.First(f => f.Code == "FEATURE_02");
        feature.Name.Should().Be("Feature Actualizada");
        feature.Description.Should().Be("Nueva descripción");
        feature.Type.Should().Be(FeatureType.Limit);
        feature.Limit.Should().Be(100);
        feature.Unit.Should().Be("items");
    }

    [Fact]
    public async Task Update_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new
        {
            Name = "Feature Actualizada",
            Description = "Nueva descripción",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PutAsJsonAsync($"/plans/{nonExistentId}/features/FEATURE_01", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithNonExistingFeature_Returns404()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id, code: "FEATURE_03");
        var request = new
        {
            Name = "Feature Actualizada",
            Description = "Nueva descripción",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/features/NON_EXISTENT", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithEmptyName_Returns422()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id, code: "FEATURE_04");
        var request = new
        {
            Name = "",
            Description = "Descripción válida",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PutAsJsonAsync($"/plans/{plan.Id}/features/FEATURE_04", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
