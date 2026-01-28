namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class AddPlanFeatureTests : PlanWebApplicationFixture
{
    public AddPlanFeatureTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task AddFeature_WithValidData_Returns201()
    {
        var plan = await CreatePlanAsync();
        var request = new
        {
            Code = "FEATURE_01",
            Name = "Feature One",
            Description = "Description",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/features", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task AddFeature_WithValidData_PersistsFeature()
    {
        var plan = await CreatePlanAsync();
        var request = new
        {
            Code = "FEATURE_PERSIST",
            Name = "Feature Persist",
            Description = "Description",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        await Client.PostAsJsonAsync($"/plans/{plan.Id}/features", request);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        result!.Features.Should().ContainSingle(f => f.Code == "FEATURE_PERSIST");
    }

    [Fact]
    public async Task AddFeature_WithLimitType_Returns201()
    {
        var plan = await CreatePlanAsync();
        var request = new
        {
            Code = "USERS_LIMIT",
            Name = "Users Limit",
            Description = "Max users",
            Type = "Limit",
            Limit = 10,
            Unit = "users"
        };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/features", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        content!.Features.Should().ContainSingle(f =>
            f.Code == "USERS_LIMIT" &&
            f.Type == FeatureType.Limit &&
            f.Limit == 10);
    }

    [Fact]
    public async Task AddFeature_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new
        {
            Code = "FEATURE_01",
            Name = "Feature One",
            Description = "Description",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PostAsJsonAsync($"/plans/{nonExistentId}/features", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddFeature_WithDuplicateCode_Returns409()
    {
        var plan = await CreatePlanAsync();
        var request = new
        {
            Code = "DUPLICATE_CODE",
            Name = "Feature One",
            Description = "Description",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        await Client.PostAsJsonAsync($"/plans/{plan.Id}/features", request);
        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/features", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddFeature_WithEmptyCode_Returns422()
    {
        var plan = await CreatePlanAsync();
        var request = new
        {
            Code = "",
            Name = "Feature One",
            Description = "Description",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/features", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task AddFeature_WithEmptyName_Returns422()
    {
        var plan = await CreatePlanAsync();
        var request = new
        {
            Code = "FEATURE_01",
            Name = "",
            Description = "Description",
            Type = "Boolean",
            Limit = (int?)null,
            Unit = (string?)null
        };

        var response = await Client.PostAsJsonAsync($"/plans/{plan.Id}/features", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
