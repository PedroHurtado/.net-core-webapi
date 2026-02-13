namespace Plans.IntegrationTests.Features.Plans.Api.PlanAggregate.Commands;

public class CreatePlanTests : PlanWebApplicationFixture
{
    public CreatePlanTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task Create_WithValidData_Returns201()
    {
        var request = new
        {
            Name = "Plan Básico",
            Description = "Ideal para empezar"
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadFromJsonAsync<PlanResponse>(JsonOptions);
        content.Should().NotBeNull();
        content!.Id.Should().NotBeEmpty();
        content.Name.Should().Be("Plan Básico");
        content.Description.Should().Be("Ideal para empezar");
        content.IsActive.Should().BeFalse();
        content.Features.Should().BeEmpty();
        content.PricingTiers.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_WithEmptyName_Returns422()
    {
        var request = new
        {
            Name = "",
            Description = "Descripción válida"
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_WithEmptyDescription_Returns422()
    {
        var request = new
        {
            Name = "Plan Válido",
            Description = ""
        };

        var response = await Client.PostAsJsonAsync("/plans", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
