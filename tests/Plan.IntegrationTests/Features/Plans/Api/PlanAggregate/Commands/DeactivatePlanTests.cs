namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class DeactivatePlanTests : PlanWebApplicationFixture
{
    public DeactivatePlanTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    private async Task<PlanResponse> CreateAndActivatePlanAsync()
    {
        var plan = await CreateCompletePlanAsync();
        var activateResponse = await Client.PostAsync($"/plans/{plan.Id}/activate", null);
        activateResponse.EnsureSuccessStatusCode();
        return (await activateResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions))!;
    }

    [Fact]
    public async Task Deactivate_WithValidData_Returns200()
    {
        var plan = await CreateAndActivatePlanAsync();

        var response = await Client.PostAsync($"/plans/{plan.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Deactivate_WithValidData_ReturnsPlanResponseWithIsActiveFalse()
    {
        var plan = await CreateAndActivatePlanAsync();

        var response = await Client.PostAsync($"/plans/{plan.Id}/deactivate", null);

        var result = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Id.Should().Be(plan.Id);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithValidData_PersistsChanges()
    {
        var plan = await CreateAndActivatePlanAsync();

        await Client.PostAsync($"/plans/{plan.Id}/deactivate", null);

        var getResponse = await Client.GetAsync($"/plans/{plan.Id}");
        var result = await getResponse.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WithNonExistingPlan_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await Client.PostAsync($"/plans/{nonExistentId}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenAlreadyInactive_Returns409()
    {
        var plan = await CreatePlanAsync();

        var response = await Client.PostAsync($"/plans/{plan.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
