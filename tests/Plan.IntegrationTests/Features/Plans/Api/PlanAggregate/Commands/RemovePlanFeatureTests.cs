namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class RemovePlanFeatureTests : PlanWebApplicationFixture
{
    public RemovePlanFeatureTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Remove_WithExistingFeature_Returns204()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id, code: "FEAT_A");
        await AddFeatureToPlanAsync(plan.Id, code: "FEAT_B");

        var response = await Client.DeleteAsync($"/plans/{plan.Id}/features/FEAT_A");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Remove_WithExistingFeature_PersistsRemoval()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id, code: "FEAT_C");
        await AddFeatureToPlanAsync(plan.Id, code: "FEAT_D");

        await Client.DeleteAsync($"/plans/{plan.Id}/features/FEAT_C");

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        result!.Features.Should().NotContain(f => f.Code == "FEAT_C");
        result.Features.Should().Contain(f => f.Code == "FEAT_D");
    }

    [Fact]
    public async Task Remove_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"/plans/{nonExistentId}/features/FEAT_X");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remove_WithNonExistingFeature_Returns404()
    {
        var plan = await CreatePlanAsync();
        await AddFeatureToPlanAsync(plan.Id, code: "FEAT_E");

        var response = await Client.DeleteAsync($"/plans/{plan.Id}/features/NON_EXISTENT");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remove_LastFeatureFromActivePlan_Returns422()
    {
        var plan = await CreateCompletePlanAsync();
        await Client.PostAsync($"/plans/{plan.Id}/activate", null);

        var response = await Client.DeleteAsync($"/plans/{plan.Id}/features/FEATURE_01");

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
